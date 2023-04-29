using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Activity.Appliaction.RequestModels;
using iSchool.Organization.Activity.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Activity.Appliaction.Service
{
    public class ComputeActivityUserEvltLikeRankCommandHandler : IRequestHandler<ComputeActivityUserEvltLikeRankCommand, ResponseResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;
        IConfiguration config;        

        public ComputeActivityUserEvltLikeRankCommandHandler(IOrgUnitOfWork unitOfWork,
            IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
            this.config = config;
        }

        public async Task<ResponseResult> Handle(ComputeActivityUserEvltLikeRankCommand cmd, CancellationToken cancellation)
        {
            var ainfo = await mediator.Send(new ActivitySimpleInfoQuery { Id = Guid.Parse(Consts.Activity1_Guid) });
            if (ainfo == null) return ResponseResult.Failed("活动无效");
            var ii = ainfo.CheckIfNotValid();
            if (ii == -1) return ResponseResult.Failed("活动未开始");
            if (ii == 1) return ResponseResult.Failed("活动已结束");

            var sql = @"
--- declare @ActivityId uniqueidentifier = '00000000-0000-0000-0000-000000000001'
---
if OBJECT_ID(N'dbo.tmp_hd1aue',N'U') is not null drop table tmp_hd1aue
------
delete from ActivityUserEvltLikeRank where IsValid=0 and datediff(minute,time,getdate())>5
delete from ActivityUserEvltLikeRank where IsValid is null
------
select a.id as activityid,s.id as specialid,s.title as specialname,e.id as evaluationid,e.title,e.likes,e.userid,e.createtime,
row_number()over(order by e.likes desc,e.createtime asc)as top_no
into tmp_hd1aue 
from Activity a left join Special s on s.activity=a.id and s.IsValid=1 and s.status=1
left join SpecialBind sb with(nolock) on sb.specialid=s.id and sb.IsValid=1
left join Evaluation e with(nolock) on e.id=sb.evaluationid and e.IsValid=1
where a.IsValid=1 and e.status=1 and a.id=@ActivityId
and isnull(a.starttime,'1970-01-01')<=getdate() and getdate()<isnull(a.endtime,'9999-12-31') --活动是否在活动期间
and isnull(a.starttime,'1970-01-01')<=e.createtime and e.createtime<isnull(a.endtime,'9999-12-31') --是否活动期间的评测
order by top_no
------ select * from tmp_hd1aue order by top_no
------
insert ActivityUserEvltLikeRank(id,activityid,userid,evaluationid,likecount,top_no,[time],IsValid)
select newid() id,activityid,userid,evaluationid,likes,top_no,getdate(),null
from tmp_hd1aue
------ select * from ActivityUserEvltLikeRank a order by IsValid desc, userid
update ActivityUserEvltLikeRank set IsValid=0 where IsValid=1 and activityid=@ActivityId
update ActivityUserEvltLikeRank set IsValid=1 where IsValid is null and activityid=@ActivityId
------
if OBJECT_ID(N'dbo.tmp_hd1aue',N'U') is not null drop table tmp_hd1aue
";
            try
            {
                await unitOfWork.DbConnection.ExecuteAsync(sql, new { ActivityId = Consts.Activity1_Guid });
            }
            catch (Exception ex)
            {
                throw new CustomResponseException("计算活动期间用户评测点赞排行error: " + ex.Message);
            }

            await redis.DelAsync(CacheKeys.Hd1_main);
            await redis.BatchDelAsync(CacheKeys.Hd1_UserEvltLikeRankData.FormatWith("*"), 30);

            return ResponseResult.Success();
        }
        
    }
}
