using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Evaluations;
using iSchool.Organization.Domain;
using MediatR;
using System.Linq;
using Microsoft.Extensions.Configuration;
using iSchool.Organization.Domain.Enum;

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 检验用户当前种草发放奖励条件是否符合
    /// </summary>
    public class EvltRewardCheckPassCommandHandler : IRequestHandler<EvltRewardCheckPassCommand, EvaluationReward>
    {
        OrgUnitOfWork _orgUnitOfWork;
        private readonly IConfiguration _config;

        public EvltRewardCheckPassCommandHandler(IOrgUnitOfWork orgUnitOfWork, IConfiguration config)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _config = config;
        }

        public async Task<EvaluationReward> Handle(EvltRewardCheckPassCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            EvaluationReward result = null;
            
            //一、种草需同时满足的条件：1-活动时间内(createtime or mtime)；2-审批状态 != 审批通过;3-$evlt.Mode == 1 && ($evlt.HasVideo || !$evlt.IsPlainText)
            var startTime = Convert.ToDateTime($"{_config["AppSettings:EvltReward:StartTime"]}");
            var endTime = Convert.ToDateTime($"{_config["AppSettings:EvltReward:EndTime"]}");
            var days = Convert.ToInt32(_config["AppSettings:EvltReward:EffectiveDays"]);

            string evltSql = $@"
select evltb.*,evlt.createtime,evlt.mtime from [dbo].[Evaluation] as evlt  left join [dbo].[EvaluationBind] as evltb
on evlt.id=evltb.evaluationid and evltb.IsValid=1
where evlt.IsValid=1 and evltb.IsValid=1 and evltb.orgid is not null and evlt.UserId=@UserId
and evlt.IsOfficial=0
and ( evlt.auditstatus<>@auditstatus  or evlt.auditstatus is null) and evlt.id=@EvltId
and ( evlt.CreateTime between @bTime and @eTime or evlt.ModifyDateTime  between @bTime and @eTime )
and evlt.mode=@mode and evlt.[status]=@status --and(evlt.HasVideo=1 or evlt.IsPlainText=0)
;";
            var dp = new DynamicParameters()
                .Set("auditstatus", EvltAuditStatusEnum.Ok.ToInt())
                .Set("EvltId", request.EvltId)
                .Set("bTime", startTime)
                .Set("eTime", endTime)
                .Set("mode", EvltContentModeEnum.Normal.ToInt())
                .Set("status", EvaluationStatusEnum.Ok.ToInt())
                .Set("UserId", request.UserId);

            var xb = _orgUnitOfWork.DbConnection.Query<EvaluationBind, (DateTime, DateTime?), (EvaluationBind, DateTime)>(evltSql, param: dp, splitOn: "createtime", 
                map: (evltB, x)=> 
                {
                    return (evltB, x.Item2 ?? x.Item1);
                }
            ).ToList();
            if (xb == null || xb.Any() == false) return result;

            //二、当前用户的品牌下奖励机会数 > 0  
            //            string countSql = $@"
            //select top 1 * from [dbo].[EvaluationReward]
            //where IsValid=1 and Used=0 and OrgId=@OrgId and UserId=@UserId and CourseId is not null and datediff(hour,ModifyDateTime,@mtime)<24*@days
            //order by CreateTime asc
            //;";
            string countSql = $@"
select top 1 * from [dbo].[EvaluationReward]
where IsValid=1 and Used=0 and OrgId=@OrgId and UserId=@UserId and CourseId is not null
order by (case when courseid=@courseid then 1 else 0 end) desc, CreateTime asc
;";

            foreach (var (item, mtime) in xb)
            {
                dp.Set("OrgId", item.Orgid);
                //dp.Set("days", days);
                dp.Set("mtime", mtime);
                dp.Set("courseid", item.Courseid);
                result = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<EvaluationReward>(countSql, dp);
                if (result != null)
                    break;
            }            
            return result;
        }
    }
}
