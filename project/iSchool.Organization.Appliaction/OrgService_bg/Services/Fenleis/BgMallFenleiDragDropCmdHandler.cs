using CSRedis;
using Dapper;
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
    public class BgMallFenleiDragDropCmdHandler : IRequestHandler<BgMallFenleiDragDropCmd, BgMallFenleiDragDropCmdResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        ILock1Factory _lock1Factory1;
        SmLogUserOperation _smLogUserOperation;

        public BgMallFenleiDragDropCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, ILock1Factory _lock1Factory1,
            SmLogUserOperation _smLogUserOperation,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._lock1Factory1 = _lock1Factory1;
            this._smLogUserOperation = _smLogUserOperation;
        }

        public async Task<BgMallFenleiDragDropCmdResult> Handle(BgMallFenleiDragDropCmd cmd, CancellationToken cancellation)
        {
            var result = new BgMallFenleiDragDropCmdResult();
            await default(ValueTask);

            if (cmd.Scode == cmd.Tcode) // no drag drop
                return result;

            if (!cmd.Tdirection.In(1, 2)) throw new CustomResponseException("参数错误");

            // get source and target
            var skv = await GetKV(cmd.Scode) ?? throw new CustomResponseException("源分类不存在或被删除了");
            var tkv = await GetKV(cmd.Tcode) ?? throw new CustomResponseException("目标分类不存在或被删除了");
            //
            if (skv.Parent != tkv.Parent) throw new CustomResponseException("无效操作.不在相同的父级分类下");
            if (skv.Sort != cmd.Ssort) throw new CustomResponseException("无效操作.数据已被更新请刷新页面");
            if (tkv.Sort != cmd.Tsort) throw new CustomResponseException("无效操作.数据已被更新请刷新页面");

            await using var _lck = await _lock1Factory1.LockAsync(
                CacheKeys.MallFenleiLck_upsort.FormatWith(skv.Parent), retry: 1
            );
            if (_lck.IsAvailable != true) throw new CustomResponseException("操作失败.");

            // find nodes
            var sql = @"
select [key] as code,sort as oldsort,-1 as newsort from KeyValue kv where kv.IsValid=1 and kv.type=@ty16 and kv.parent=@pcode 
and kv.sort>=@s1 and kv.sort<=@s2
order by sort
";
            var ls = (await _orgUnitOfWork.DbConnection.QueryAsync<SortDto>(sql, new
            {
                ty16 = Consts.Kvty_MallFenlei,
                pcode = skv.Parent,
                s1 = Math.Min(cmd.Ssort, cmd.Tsort),
                s2 = Math.Max(cmd.Ssort, cmd.Tsort),
            })).AsList();

            if (ls.Count < 2) return result;

            // re sort
            // 相同父节点的子节点的sort应该是不同的
            var isddown = cmd.Ssort < cmd.Tsort; // 向下拖
            if (isddown)
            {
                var j = cmd.Tdirection == 1 ? ls.Count - 2  // 目标前面
                    : cmd.Tdirection == 2 ? ls.Count - 1    // 目标后面
                    : ls.Count - 1;
                              
                for (var i = 0; i <= j; i++)
                {
                    ls[i].NewSort = ls[i == 0 ? j : i - 1].OldSort;
                }
            }
            else // 向上拖
            {
                var j = cmd.Tdirection == 1 ? 0  // 目标前面
                    : cmd.Tdirection == 2 ? 1    // 目标后面
                    : 0;

                for (var i = ls.Count - 1; i >= j; i--)
                {
                    ls[i].NewSort = ls[i == ls.Count - 1 ? j : i + 1].OldSort;
                }
            }

            ls.RemoveAll(_ => _.NewSort == -1 || _.NewSort == _.OldSort);
            if (ls.Count < 1) return result;

            //
            // up db
            sql = $@"
update [KeyValue] set [sort]=@NewSort where IsValid=1 and type={Consts.Kvty_MallFenlei} and [key]=@Code and [sort]=@OldSort
";
            await _orgUnitOfWork.ExecuteAsync(sql, ls);

            // clear cache
            await _mediator.Send(new ClearRedisCacheCmd { Keys = new[] { CacheKeys.MallFenlei_DelFontKeys } });

            // add user log
            _smLogUserOperation.SetUserId(cmd.UserId)
                .SetClass(nameof(BgMallFenleiDragDropCmd))
                .SetParams("_", cmd)                
                .SetOldata("ls", ls)
                .SetTime(DateTime.Now);

            result.NewSorts = ls.Select(_ => (_.Code, _.NewSort)).ToArray();
            return result;
        }

        async Task<KeyValue> GetKV(int code)
        {
            var sql = $"select top 1 * from [keyvalue] where IsValid=1 and [type]=@ty16 and [key]=@code";
            var kv = await _orgUnitOfWork.QueryFirstOrDefaultAsync<KeyValue>(sql, new { code, ty16 = Consts.Kvty_MallFenlei });
            return kv;
        }

        class SortDto
        {
            public int Code { get; set; }
            public int OldSort { get; set; }
            public int NewSort { get; set; }
        }
    }
}
