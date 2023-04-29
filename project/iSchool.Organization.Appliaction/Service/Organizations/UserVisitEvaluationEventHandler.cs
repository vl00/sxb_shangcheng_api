using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class UserVisitEvaluationEventHandler : INotificationHandler<UserVisitEvaluationEvent>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;

        public UserVisitEvaluationEventHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
        }

        public async Task Handle(UserVisitEvaluationEvent e, CancellationToken cancellation)
        {
            await default(ValueTask);

            var rdk = CacheKeys.UV_evlt_user.FormatWith(("date", e.Now.Date.ToString("yyyy-MM-dd")),
                ("userid", e.UserId), ("evltid", e.EvltId));
            var b = await redis.SetAsync(rdk, 1, 60 * 60 * 24 * 1, RedisExistence.Nx);
            if (!b) return;

            rdk = CacheKeys.UV_evlt_total.FormatWith(("date", e.Now.Date.ToString("yyyy-MM-dd")), ("evltid", e.EvltId));
            var rr = await redis.StartPipe()
                .IncrBy(rdk)
                .Ttl(rdk)
                .Set(CacheKeys.UV_evlt_diff.FormatWith(("date", e.Now.Date.ToString("yyyy-MM-dd")), ("evltid", e.EvltId)), 1, 60 * 60 * 2)
                .EndPipeAsync();

            if (Equals(rr[1], -1L))
            {
                _ = redis.ExpireAsync(rdk, TimeSpan.FromDays(1).Add(TimeSpan.FromMinutes(1)));
            }
        }

    }
}
