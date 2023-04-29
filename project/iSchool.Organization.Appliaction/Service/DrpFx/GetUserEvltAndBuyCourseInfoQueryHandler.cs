using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class GetUserEvltAndBuyCourseInfoQueryHandler : IRequestHandler<GetUserEvltAndBuyCourseInfoQuery, IEnumerable<UserEvltAndBuyCourseInfoDto>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;        
        IMapper _mapper;
        IConfiguration _config;

        public GetUserEvltAndBuyCourseInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;            
            this._redis = redis;            
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<IEnumerable<UserEvltAndBuyCourseInfoDto>> Handle(GetUserEvltAndBuyCourseInfoQuery query, CancellationToken cancellation)
        {
            var result = query.UserIds.Select(uid => new UserEvltAndBuyCourseInfoDto { UserId = uid }).ToArray();
            await default(ValueTask);

            // EvltCount + StickEvltCount
            {
                var sql = $@"
select e.userid,count(1),sum(case when e.stick=1 then 1 else 0 end) 
from Evaluation e where e.IsValid=1 and e.[status]={EvaluationStatusEnum.Ok.ToInt()}
and e.userid in @UserIds
group by e.userid
";
                var ls = await _orgUnitOfWork.QueryAsync<(Guid, int, int)>(sql, new { query.UserIds });
                foreach (var dto in result)
                {
                    if (!ls.TryGetOne(out var x, _ => _.Item1 == dto.UserId)) continue;
                    dto.EvltCount = x.Item2;
                    dto.StickEvltCount = x.Item3;
                }
            }

            // BuyCourseCount + ConsumedMoneys
            {
                var sql = $@"
select o.userid,sum(d.number),sum(d.number*d.price) --,sum(o.totalpayment),sum(o.freight)
from [order] o join [OrderDetial] d on d.orderid=o.id
where o.IsValid=1 and o.type>={OrderType.BuyCourseByWx.ToInt()}
and (o.status in({OrderStatusV2.Completed.ToInt()},{OrderStatusV2.Paid.ToInt()}) or (o.status>300 and o.status<400))
and o.userid in @UserIds
group by o.userid
";
                var ls = await _orgUnitOfWork.QueryAsync<(Guid, int, decimal)>(sql, new { query.UserIds });
                foreach (var dto in result)
                {
                    if (!ls.TryGetOne(out var x, _ => _.Item1 == dto.UserId)) continue;
                    dto.BuyCourseCount = x.Item2;
                    dto.ConsumedMoneys = x.Item3;
                }
            }

            return result;
        }

    }
}
