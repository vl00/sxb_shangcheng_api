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
    public class PVisitEventHandler : INotificationHandler<PVisitEvent>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;

        public PVisitEventHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
        }

        public async Task Handle(PVisitEvent e, CancellationToken cancellation)
        {
            await default(ValueTask);

            var rdk = CacheKeys.PV_total.FormatWith(
                ("date", e.Now.Date.ToString("yyyy-MM-dd")),
                ("cttid", e.CttId),
                ("ctt", e.CttType.GetName())
            );
            using var pipe = redis.StartPipe();
            var rr = await pipe.IncrBy(rdk).Ttl(rdk)
                .Set(CacheKeys.PV_diff.FormatWith(("cttid", e.CttId), ("ctt", e.CttType.GetName())), 1, 60 * 5)
                .EndPipeAsync();

            if (Equals(rr[1], -1L))
            {
                await redis.ExpireAsync(rdk, TimeSpan.FromDays(1).Add(TimeSpan.FromMinutes(1)));
            }
        }

    }
}
