using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services
{
    public class BgMallFenleiSaveCmdHandler : IRequestHandler<BgMallFenleiSaveCmd, BgMallFenleiSaveCmdResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        ILock1Factory _lock1Factory1;
        SmLogUserOperation _smLogUserOperation;

        public BgMallFenleiSaveCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, ILock1Factory lock1Factory,
            SmLogUserOperation smLogUserOperation,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._lock1Factory1 = lock1Factory;
            this._smLogUserOperation = smLogUserOperation;
        }

        public async Task<BgMallFenleiSaveCmdResult> Handle(BgMallFenleiSaveCmd cmd, CancellationToken cancellation)
        {
            var result = new BgMallFenleiSaveCmdResult();
            await default(ValueTask);

            if (cmd.UserId == default)
            {
                throw new CustomResponseException("请登录", 401);
            }
            cmd.Name = cmd.Name?.Trim();
            if (string.IsNullOrEmpty(cmd.Name))
            {
                throw new CustomResponseException("请填写分类标题");
            }

            var isadd = cmd.Code == null && cmd.Pcode != null ? true
                : cmd.Code != null && cmd.Pcode == null ? false      // 暂不支持修改归属父节点 
                : throw new CustomResponseException("参数错误");

            if (isadd) await DoAdd(result, cmd);
            else await DoUpdate(result, cmd);

            // clear cache
            await _mediator.Send(new ClearRedisCacheCmd 
            { 
                Keys = new[] 
                { 
                    CacheKeys.MallFenlei_DelFontKeys,
                    "org:course:*",
                    "org:courses:*",
                    "org:organizations:*",
                    "org:organization:*",
                } 
            });

            return result;
        }

        async Task DoAdd(BgMallFenleiSaveCmdResult result, BgMallFenleiSaveCmd cmd)
        {
            var pcode = cmd.Pcode!.Value;
            var parent = await GetKV(pcode) ?? throw new CustomResponseException("父分类不存在或被删除了");
            if (parent.Depth is null || parent.Depth >= Consts.MallFenlei_MaxDepth)
            {
                throw new CustomResponseException($"商城分类最多{Consts.MallFenlei_MaxDepth}层级");
            }
            //if (await CheckName(cmd.Name))
            //{
            //    throw new CustomResponseException("添加失败,已存在同名分类");
            //}

            await using var _lck1 = await _lock1Factory1.LockAsync(CacheKeys.MallFenleiLck_saveadd, retry: 1);
            if (_lck1.IsAvailable != true) throw new CustomResponseException("添加失败,可能存在多人操作");

            var kv = new KeyValue();
            kv.Name = cmd.Name;
            kv.Type = Consts.Kvty_MallFenlei;
            kv.IsValid = true;
            kv.Parent = pcode;
            kv.Depth = parent.Depth!.Value + 1;
            kv.Attach = cmd.Img;
            //
            kv.Key = await GetNewCode();
            kv.Sort = await GetMaxSortByPcode(pcode) + 1;
            //
            try
            {
                var i = await _orgUnitOfWork.DbConnection.InsertAsync(kv);
                if (i < 1) throw new Exception("添加失败");
            }
            catch (Exception ex)
            {
                throw new CustomResponseException(ex.Message);
            }

            // add user log
            _smLogUserOperation.SetUserId(cmd.UserId)
                .SetClass(nameof(BgMallFenleiSaveCmd)).SetMethod(nameof(DoAdd))
                .SetParams("_", cmd).SetParams("new", kv)
                .SetTime(DateTime.Now);

            result.Code = kv.Key;
            result.Sort = kv.Sort.Value;
            result.Depth = kv.Depth.Value;
        }

        async Task DoUpdate(BgMallFenleiSaveCmdResult result, BgMallFenleiSaveCmd cmd)
        {
            var code = cmd.Code!.Value;
            var self = await GetKV(code) ?? throw new CustomResponseException("分类不存在或被删除了");

            //if (await CheckName(cmd.Name, code))
            //{
            //    throw new CustomResponseException("更新失败,已存在同名分类");
            //}

            await using var _lck1 = await _lock1Factory1.LockAsync(CacheKeys.MallFenleiLck_saveup.FormatWith(code), retry: 1);
            if (_lck1.IsAvailable != true) throw new CustomResponseException("更新失败,可能存在多人操作");

            try
            {
                var sql = "update [KeyValue] set name=@Name,Attach=@Img where type=@ty16 and IsValid=1 and [key]=@code";
                var i = await _orgUnitOfWork.ExecuteAsync(sql, new { code, ty16 = Consts.Kvty_MallFenlei, cmd.Name, cmd.Img });
                if (i < 1) throw new CustomResponseException("更新失败");
            }
            catch (Exception ex)
            {
                throw new CustomResponseException(ex.Message);
            }

            // add user log
            _smLogUserOperation.SetUserId(cmd.UserId)
                .SetClass(nameof(BgMallFenleiSaveCmd)).SetMethod(nameof(DoUpdate))
                .SetParams("_", cmd)
                .SetOldata("keyvalue", self)
                .SetTime(DateTime.Now);

            result.Code = self.Key;
            result.Sort = self.Sort ?? 0;
            result.Depth = self.Depth ?? 0;
        }

        async Task<KeyValue> GetKV(int code)
        {
            var sql = $"select top 1 * from [keyvalue] where IsValid=1 and [type]=@ty16 and [key]=@code";
            var kv = await _orgUnitOfWork.QueryFirstOrDefaultAsync<KeyValue>(sql, new { code, ty16 = Consts.Kvty_MallFenlei });
            return kv ?? (code == 0 ? new KeyValue { Key = 0, Parent = 0, Depth = 0 } : null);
        }

        /// <summary>获取同级中最大的排序</summary>
        async Task<int> GetMaxSortByPcode(int pcode)
        {
            var sql = $"select max(sort) from KeyValue kv where 1=1  and type=@ty16 and [parent]=@pcode "; //-- and IsValid = 1
            var v = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<int?>(sql, new { pcode, ty16 = Consts.Kvty_MallFenlei });
            return v ?? 0;
        }

        /// <summary>新建时的code</summary>
        async Task<int> GetNewCode()
        {
            var sql = $"select max([key]) from KeyValue kv where 1=1  and type=@ty16 "; //-- and IsValid = 1
            var v = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<int?>(sql, new { ty16 = Consts.Kvty_MallFenlei });
            return (v ?? 0) + 1;
        }

        async Task<bool> CheckName(string name, int? code = null)
        {
            var sql = $"select top 1 * from [keyvalue] where IsValid=1 and [type]=@ty16 and [Name]=@name {"and [key]<>@code".If(code != null)} ";
            var kv = await _orgUnitOfWork.QueryFirstOrDefaultAsync<KeyValue>(sql, new { name, code, ty16 = Consts.Kvty_MallFenlei });
            return kv != null;
        }
    }
}
