using CSRedis;
using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class UserHd2ActiQueryHandler : IRequestHandler<UserHd2ActiQuery, UserHd2ActiQueryResult>
    {                    
        IMediator mediator;
        CSRedisClient redis;
        OrgUnitOfWork orgUnitOfWork;

        public UserHd2ActiQueryHandler(CSRedisClient redis, IOrgUnitOfWork orgUnitOfWork,
            IMediator mediator)
        {                        
            this.mediator = mediator;
            this.redis = redis;
            this.orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
        }

        public async Task<UserHd2ActiQueryResult> Handle(UserHd2ActiQuery query, CancellationToken cancellationToken)
        {
            var result = new UserHd2ActiQueryResult();
            var now = query.Now ?? DateTime.Now;
            var mobile = string.Empty;
            var others = query.OtherUserIds;

            if (others == null)
            {
                var u = (await mediator.Send(new UserMobileInfoQuery { UserIds = new[] { query.UserId } })).FirstOrDefault();
                if (u.UserInfo == null) throw new CustomResponseException("no user");
                mobile = u.UserInfo.Mobile;
                others = u.OtherUserInfo.Select(_ => _.Id).ToArray();
            }
            if (query.UserId != default && others != null)
            {
                var sql = $@"
select count(1)as c1,sum(case when e.userid=@UserId then 1 else 0 end)as c2,sum(case when e.userid<>@UserId then 1 else 0 end)as c3,
sum(case when datediff(dd,aeb.Mtime,@now)=0 then 1 else 0 end)as c4,
sum(case when datediff(dd,aeb.Mtime,@now)=0 and e.userid=@UserId then 1 else 0 end)as c5,
sum(case when datediff(dd,aeb.Mtime,@now)=0 and e.userid<>@UserId then 1 else 0 end)as c6
from ActivityEvaluationBind aeb with(nolock)	
left join Activity a on a.id=aeb.activityid 
left join Evaluation e on e.id=aeb.evaluationid 
where aeb.IsValid=1 and aeb.IsLatest=1 and a.id=@ActivityId
and a.IsValid=1 and a.type={ActivityType.Hd2.ToInt()} and a.status={ActivityStatus.Ok.ToInt()} 
-- and e.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()} 
-- and aeb.status<>{ActiEvltAuditStatus.Not.ToInt()}
and (e.userid=@UserId {(others.Length < 1 ? "" : "or e.userid in @others")} --or aeb.mobile=@mobile 
) 
";
                var q = (await orgUnitOfWork.QueryAsync<(int, int, int), (int, int, int), (int c1, int c2, int c3, int c4, int c5, int c6)>(
                    sql, param: new { query.ActivityId, query.UserId, mobile, others, now }, splitOn: "c4",
                    map: (i1, i2) => (i1.Item1, i1.Item2, i1.Item3, i2.Item1, i2.Item2, i2.Item3)
                )).FirstOrDefault();

                result.Allcount = q.c1;
                result.Ucount = q.c2;
                result.Ocount = q.c3;
                result.Allcount_now = q.c4;
                result.Ucount_now = q.c5;
                result.Ocount_now = q.c6;
            }
            result.UserId = query.UserId;
            result.OtherUserIds = others;
            result.Mobile = mobile;
            result.ActivityId = query.ActivityId;

            return result;
        }
    }
}
