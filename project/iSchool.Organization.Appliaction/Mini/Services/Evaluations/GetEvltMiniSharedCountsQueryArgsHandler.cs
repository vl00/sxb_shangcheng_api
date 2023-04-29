using CSRedis;
using Dapper;
using iSchool;
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
    public class GetEvltMiniSharedCountsQueryArgsHandler : IRequestHandler<GetEvltMiniSharedCountsQueryArgs, IEnumerable<(Guid EvltId, int SharedCount)>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        IUserInfo me;
        CSRedisClient redis;              

        public GetEvltMiniSharedCountsQueryArgsHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;            
        }

        public async Task<IEnumerable<(Guid EvltId, int SharedCount)>> Handle(GetEvltMiniSharedCountsQueryArgs query, CancellationToken cancellation)
        {
            if (query.EvltIds?.Length < 1) return Enumerable.Empty<(Guid, int)>();

            var result = new List<(Guid EvltId, int SharedCount)>();
            var ls_notIn = new List<Guid>(); // not in caches

            if (true)
            {
                using var pipe = redis.StartPipe();
                foreach (var id in query.EvltIds)
                {
                    pipe.Get(CacheKeys.MiniEvltSharedCount.FormatWith(id));
                }
                var rr = await pipe.EndPipeAsync();
                for (var i = 0; i < rr.Length; i++)
                {
                    var c = int.TryParse(rr[i]?.ToString(), out var _c) && _c > -1 ? _c : -1;
                    if (c >= 0) result.Add((query.EvltIds[i], c));
                    else ls_notIn.Add(query.EvltIds[i]);
                }
            }

            // 查db查出不在cache的
            if (ls_notIn.Count > 0)
            {
                var sql = $@"
select e.id as Item1,isnull(e.SharedTime,0) as Item2 from Evaluation e 
where e.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()}
and e.id in @evltIds ";
                var dys = await _orgUnitOfWork.QueryAsync<(Guid, int)>(sql, new { evltIds = ls_notIn });
                result.AddRange(dys);

                using var pipe = redis.StartPipe();
                foreach (var dy in dys)
                    pipe.Set(CacheKeys.MiniEvltSharedCount.FormatWith(dy.Item1), dy.Item2, 60 * 60 * 24, RedisExistence.Nx);
                await pipe.EndPipeAsync();
            }
            ls_notIn.Clear();

            return result;
        }

    }
}
