using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
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

namespace iSchool.Organization.Appliaction.Services
{
    public class RwInviteActivityUserOrderStoreStatusLsQueryHandler : IRequestHandler<RwInviteActivityUserOrderStoreStatusLsQuery, IEnumerable<RwInviteActivityUserOrderStoreStatusLsItem>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public RwInviteActivityUserOrderStoreStatusLsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<IEnumerable<RwInviteActivityUserOrderStoreStatusLsItem>> Handle(RwInviteActivityUserOrderStoreStatusLsQuery query, CancellationToken cancellation)
        {
            var sql = $@"
select o.id as OrderId,o.code as OrderNo,o.CreateTime,o.status,min(p.id)as OrderDetailId,sum(Number)as Count,sum(try_convert(float,json_value(p.ctn,'$._RwInviteActivity.consumedScores')))as consumedScores
from [order] o left join OrderDetial p on o.id=p.orderid
where o.type>=2 {"and o.CreateTime>=@StartTime".If(query.StartTime != default)} {"and o.CreateTime<@EndTime".If(query.EndTime != default)}
and o.userid=@UserId
and (o.status={OrderStatusV2.Paid.ToInt()} or o.status>300)
and json_value(p.ctn,'$._RwInviteActivity.consumedScores') is not null
group by o.id,o.code,o.CreateTime,o.status
order by o.CreateTime desc
";
            var ls = (await _orgUnitOfWork.QueryAsync<RwInviteActivityUserOrderStoreStatusLsItem>(sql, new 
            {
                query.UserId,
                StartTime = query.StartTime.Date,
                EndTime = query.EndTime.AddDays(1).Date,
            })).AsArray();

            foreach (var item in ls)
            {
                item.StatusDesc = ((OrderStatusV2)item.Status).GetDesc();
            }

            return ls;
        }

    }
}
