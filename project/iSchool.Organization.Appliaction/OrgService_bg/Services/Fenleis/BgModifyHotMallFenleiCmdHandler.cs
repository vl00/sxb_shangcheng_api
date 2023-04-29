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
    public class BgModifyHotMallFenleiCmdHandler : IRequestHandler<BgModifyHotMallFenleiCmd, object>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        ILock1Factory _lock1Factory1;

        public BgModifyHotMallFenleiCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, ILock1Factory _lock1Factory1,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._lock1Factory1 = _lock1Factory1;
        }

        public async Task<object> Handle(BgModifyHotMallFenleiCmd cmd, CancellationToken cancellation)
        {            
            await default(ValueTask);

            if (!cmd.Index.In(1, 2, 3)) throw new CustomResponseException("参数错误");
            if (!cmd.IsDeleted && cmd.D3 < 1) throw new CustomResponseException("参数错误");

            await using var _lck = await _lock1Factory1.LockAsync(
                CacheKeys.MallFenleiLck_edithot, retry: 1
            );
            if (_lck.IsAvailable != true) throw new CustomResponseException("操作失败.");

            if (cmd.IsDeleted) await DoDelete(cmd);
            else await DoEdit(cmd);

            // clear cache
            await _mediator.Send(new ClearRedisCacheCmd { Keys = new[] { CacheKeys.MallFenlei_DelFontKeys } });

            return Unit.Value;
        }

        async Task DoDelete(BgModifyHotMallFenleiCmd cmd)
        {
            var sql = $@"
update PopularClassify set IsValid=0,ModifyDateTime=getdate(),Modifier=@UserId where IsValid=1 and Sort=@Index
";
            await _orgUnitOfWork.ExecuteAsync(sql, new { cmd.Index, cmd.UserId });
        }

        async Task DoEdit(BgModifyHotMallFenleiCmd cmd)
        {
            var sql = $"select top 1 * from [keyvalue] where IsValid=1 and [type]=@ty16 and [key]=@code";
            var kv = await _orgUnitOfWork.QueryFirstOrDefaultAsync<KeyValue>(sql, new { code = cmd.D3, ty16 = Consts.Kvty_MallFenlei });
            if (kv == null) throw new CustomResponseException("分类不存在或被删除了");
            if (kv.Depth != 3) throw new CustomResponseException("不是三级分类");

            sql = $"select * from PopularClassify where IsValid=1 and ClassifyKey=@code";
            var pcy = await _orgUnitOfWork.QueryAsync<PopularClassify>(sql, new { code = cmd.D3 });
            if (pcy.Any() && pcy.Any(p => p.Sort != cmd.Index))
            {
                throw new CustomResponseException("热门分类不能重复添加");
            }

            try
            {
                _orgUnitOfWork.BeginTransaction();

                sql = $@"update PopularClassify set IsValid=0,ModifyDateTime=getdate(),Modifier=@UserId where IsValid=1 and Sort=@Index  ";
                var i = await _orgUnitOfWork.ExecuteAsync(sql, new { cmd.UserId, cmd.Index, cmd.D3 }, _orgUnitOfWork.DbTransaction);

                var dto = new PopularClassify { IsValid = true };
                dto.Id = Guid.NewGuid();
                dto.ClassifyKey = cmd.D3;
                dto.Sort = cmd.Index;
                dto.CreateTime = DateTime.Now;
                dto.ModifyDateTime = dto.CreateTime;
                dto.Creator = cmd.UserId;
                dto.Modifier = dto.Creator;
                await _orgUnitOfWork.DbConnection.InsertAsync(dto, _orgUnitOfWork.DbTransaction);

                _orgUnitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.SafeRollback();
                throw new CustomResponseException(ex.Message);
            }
        }

    }
}
