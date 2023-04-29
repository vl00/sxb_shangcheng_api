using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class PcGetOrgsCountsQueryHandler : IRequestHandler<PcGetOrgsCountsQuery, PcGetOrgsCountsQueryResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;        
        CSRedisClient redis;                

        public PcGetOrgsCountsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;            
            this.redis = redis;                        
        }

        public async Task<PcGetOrgsCountsQueryResult> Handle(PcGetOrgsCountsQuery query, CancellationToken cancellation)
        {
            var result = new PcGetOrgsCountsQueryResult();
            await default(ValueTask);

            if (query.OrgIds?.Any() != true) return result;

            var orgIds = query.OrgIds.ToArray();
            var ls_notIn = new List<Guid>();
            string sql = null;

            foreach (var id in orgIds)
            {
                result[id] = default;
            }

            //
            // find CourceCount
            //
            if (true)
            {
                using var pipe = redis.StartPipe();
                foreach (var id in orgIds)                
                    pipe.Get(CacheKeys.PC_OrgCounts_Course.FormatWith(id));                
                var rr = await pipe.EndPipeAsync();

                for (var i = 0; i < rr.Length; i++)
                {
                    var c = int.TryParse(rr[i]?.ToString(), out var _c) ? _c : -1;
                    if (c == -1) ls_notIn.Add(orgIds[i]);
                    else
                    {
                        var p = result[orgIds[i]];
                        p.CourceCount = c;
                        result[orgIds[i]] = p;
                    }
                }
            }
            if (ls_notIn.Count > 0)
            {
                sql = $@"
select o.id as Item1,count(1) as Item2 
from Course c join Organization o on c.orgid=o.id
where o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} and c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and c.IsInvisibleOnline=0
and o.id in @ids
group by o.id
";
                var qs = await unitOfWork.QueryAsync<(Guid, int)>(sql, new { ids = ls_notIn });
                foreach (var (id, c) in qs)
                {
                    if (!result.TryGetValue(id, out var r)) continue;
                    r.CourceCount = c;
                    result[id] = r;
                }

                using var pipe = redis.StartPipe();
                foreach (var id in ls_notIn)
                {
                    pipe.Ttl(CacheKeys.PC_OrgCounts_Course.FormatWith(id));
                    pipe.Set(CacheKeys.PC_OrgCounts_Course.FormatWith(id), result[id].CourceCount);
                }
                var rr = await pipe.EndPipeAsync();
                for (var i = 0; i < rr.Length; i += 2)
                {
                    if (rr[i] is -1L || rr[i] is -2L)
                        _ = redis.ExpireAsync(CacheKeys.PC_OrgCounts_Course.FormatWith(ls_notIn[i / 2]), 60 * 60 * 2);
                }
            }
            ls_notIn.Clear();

            //
            // find EvaluationCount
            //
            if (true)
            {
                using var pipe = redis.StartPipe();
                foreach (var id in orgIds)
                    pipe.Get(CacheKeys.PC_OrgCounts_Evlt.FormatWith(id));
                var rr = await pipe.EndPipeAsync();

                for (var i = 0; i < rr.Length; i++)
                {
                    var c = int.TryParse(rr[i]?.ToString(), out var _c) ? _c : -1;
                    if (c == -1) ls_notIn.Add(orgIds[i]);
                    else
                    {
                        var p = result[orgIds[i]];
                        p.EvaluationCount = c;
                        result[orgIds[i]] = p;
                    }
                }
            }
            if (ls_notIn.Count > 0)
            {
                sql = $@"
select id,count(1) as cc from(
select o.id,eb.evaluationid from Organization o
left join EvaluationBind eb on eb.IsValid=1 and eb.orgid=o.id
left join Evaluation e on e.IsValid=1 and e.id=eb.evaluationid
where o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} and e.status={EvaluationStatusEnum.Ok.ToInt()}
and o.id in @ids
group by o.id,eb.evaluationid
)T group by id
";
                var qs = await unitOfWork.QueryAsync<(Guid, int)>(sql, new { ids = ls_notIn });
                foreach (var (id, c) in qs)
                {
                    if (!result.TryGetValue(id, out var r)) continue;
                    r.EvaluationCount = c;
                    result[id] = r;
                }

                using var pipe = redis.StartPipe();
                foreach (var id in ls_notIn)
                {
                    pipe.Ttl(CacheKeys.PC_OrgCounts_Evlt.FormatWith(id));
                    pipe.Set(CacheKeys.PC_OrgCounts_Evlt.FormatWith(id), result[id].EvaluationCount);
                }
                var rr = await pipe.EndPipeAsync();
                for (var i = 0; i < rr.Length; i += 2)
                {
                    if (rr[i] is -1L || rr[i] is -2L)
                        _ = redis.ExpireAsync(CacheKeys.PC_OrgCounts_Evlt.FormatWith(ls_notIn[i / 2]), 60 * 60 * 2);
                }
            }
            ls_notIn.Clear();

            //
            // find GoodsCount
            //
            if (true)
            {
                using var pipe = redis.StartPipe();
                foreach (var id in orgIds)
                    pipe.Get(CacheKeys.Mp_OrgCounts_Goods.FormatWith(id));
                var rr = await pipe.EndPipeAsync();

                for (var i = 0; i < rr.Length; i++)
                {
                    var c = int.TryParse(rr[i]?.ToString(), out var _c) ? _c : -1;
                    if (c == -1) ls_notIn.Add(orgIds[i]);
                    else
                    {
                        var p = result[orgIds[i]];
                        p.GoodsCount = c;
                        result[orgIds[i]] = p;
                    }
                }
            }
            if (ls_notIn.Count > 0)
            {
                sql = $@"
select o.id, count(g.id) as cc
from Organization o
left join Course c on o.id=c.orgid and c.IsValid=1 
left join CourseGoods g on g.Courseid=c.id and g.IsValid=1
where o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} and c.status={CourseStatusEnum.Ok.ToInt()} and c.IsInvisibleOnline=0 and g.show=1
and o.id in @ids
group by o.id
";
                var qs = await unitOfWork.QueryAsync<(Guid, int)>(sql, new { ids = ls_notIn });
                foreach (var (id, c) in qs)
                {
                    if (!result.TryGetValue(id, out var r)) continue;
                    r.GoodsCount = c;
                    result[id] = r;
                }

                using var pipe = redis.StartPipe();
                foreach (var id in ls_notIn)
                {
                    pipe.Ttl(CacheKeys.Mp_OrgCounts_Goods.FormatWith(id));
                    pipe.Set(CacheKeys.Mp_OrgCounts_Goods.FormatWith(id), result[id].GoodsCount);
                }
                var rr = await pipe.EndPipeAsync();
                for (var i = 0; i < rr.Length; i += 2)
                {
                    if (rr[i] is -1L || rr[i] is -2L)
                        _ = redis.ExpireAsync(CacheKeys.Mp_OrgCounts_Goods.FormatWith(ls_notIn[i / 2]), 60 * 60 * 2);
                }
            }
            ls_notIn.Clear();

            //
            // end
            return result;
        }
        
    }
}
