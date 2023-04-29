using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.SchoolsApis
{
    public class GetRwInviteActivityOrderCountQueryHandler : IRequestHandler<GetRwInviteActivityOrderCountQuery, GetRwInviteActivityOrderCountQryResultItem[]>
    {
        IConfiguration _config;
        CSRedisClient _redis;
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;

        public GetRwInviteActivityOrderCountQueryHandler(IConfiguration config, CSRedisClient redis, IOrgUnitOfWork orgUnitOfWork,
            IMediator mediator)
        {
            this._config = config;
            this._redis = redis;
            this._mediator = mediator;
            this._orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
        }

        public async Task<GetRwInviteActivityOrderCountQryResultItem[]> Handle(GetRwInviteActivityOrderCountQuery query, CancellationToken cancellation)
        {
            var courseIds = await _redis.SMembersAsync<Guid>(CacheKeys.RwInviteActivity_InvisibleOnlineCourses);
            if ((courseIds?.Length ?? 0) < 1) return new GetRwInviteActivityOrderCountQryResultItem[0];

            var sql = $@"
select o.userid,min(u.nickname) as u_nickname,o.unionID,min(un.nickname) as un_nickname,count(1) as [count] 
from (
select o.createtime,o.code,o.userid,o.type,json_value(p.ctn,'$._RwInviteActivity.unionID')as unionID,
json_value(p.ctn,'$._RwInviteActivity.courseExchange.type')as courseExchange_type,
try_convert(float,json_value(p.ctn,'$._RwInviteActivity.consumedScores'))as consumed_scores,
p.courseid,p.ctn
from [order] o join OrderDetial p on o.id=p.orderid
where o.IsValid=1 and o.type>=2 and (o.status=103 or o.status>300) 
and p.courseid in @courseIds
)o left join [iSchoolUser].dbo.userinfo u on u.id=o.userid
left join [iSchoolUser].dbo.unionid_weixin un on un.valid=1 and un.userid=o.userid
where o.unionID is not null {"and o.courseExchange_type=@CourseExchangeType".If(query.CourseExchangeType != null)}
{"and o.unionID in @UnionIDs".If(query.UnionIDs?.Length > 0)}
group by o.userid,o.unionID
";
            var datas = (await _orgUnitOfWork.QueryAsync<GetRwInviteActivityOrderCountQryResultItem>(sql, new
            {
                courseIds,
                query.CourseExchangeType,
                query.UnionIDs,
            })).AsArray();

            return datas;
        }


    }
}
