using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class MiniEvltUpSharedCountsCmdHandler : IRequestHandler<MiniEvltUpSharedCountsCmd, bool>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        private readonly IUserInfo me;
        private readonly CSRedisClient redis;
        private readonly ILock1Factory _lck1fay;

        public MiniEvltUpSharedCountsCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, ILock1Factory lck1fay,
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;
            this._lck1fay = lck1fay;
        }

        public async Task<bool> Handle(MiniEvltUpSharedCountsCmd cmd, CancellationToken cancellation)
        {
            var evlt = await _mediator.Send(new GetEvltBaseInfoQuery { EvltId = cmd.EvltId });
            if (evlt == null) return false;

            // 限制
            var hi = await redis.IncrByAsync(CacheKeys.MiniDoEvltSharedIncr.FormatWith(me.UserId, cmd.EvltId), 1);
            if (hi == 1)
            {
                _ = redis.ExpireAsync(CacheKeys.MiniDoEvltSharedIncr.FormatWith(me.UserId, cmd.EvltId), 10);
            }
            if (hi > 3)
            {
                throw new CustomResponseException("请不要频繁操作");
            }

            await using var lck0 = await _lck1fay.LockAsync($"org:lck2:up_evlt_minishared:evltid_{cmd.EvltId}");
            if (!lck0.IsAvailable) throw new CustomResponseException("系统繁忙", 555);

            // incr sharedcount
            var sharedcount0 = (await _mediator.Send(new GetEvltMiniSharedCountsQueryArgs(cmd.EvltId))).FirstOrDefault().SharedCount;
            var sharedcount1 = await redis.IncrByAsync(CacheKeys.MiniEvltSharedCount.FormatWith(cmd.EvltId), 1);

            // up db
            {
                var sql = "update Evaluation set SharedTime=@sharedcount1 where id=@EvltId";
                await _orgUnitOfWork.ExecuteAsync(sql, new { cmd.EvltId, sharedcount1 });
            }

            return true;
        }

    }
}
