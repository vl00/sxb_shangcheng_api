using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels.Evaluations;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{

    /// <summary>
    /// 评测详情
    /// </summary>
    public class EvaluationDetailsQueryHandler : IRequestHandler<EvaluationDetailsQuery, EvaluationDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IHttpClientFactory httpClientFactory;
        IMediator _mediator;

        public EvaluationDetailsQueryHandler(IOrgUnitOfWork unitOfWork, IHttpClientFactory _httpClientFactory, IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this.httpClientFactory = _httpClientFactory;
            _mediator = mediator;
        }

        public async Task<EvaluationDto> Handle(EvaluationDetailsQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var dy = new DynamicParameters();
            dy.Set("Id", request.Id);
            var dto = new EvaluationDto();
            string sql = $@" 
select e.*,item.content,item.pictures,e.auditstatus,item.video,item.videoCover
--e.id,e.title,item.content,item.pictures,type,IsOfficial 
from [dbo].[EvaluationItem]  item left join [dbo].[Evaluation] e
on item.evaluationid=e.id
where item.IsValid=1 and e.IsValid=1 and item.evaluationid=@Id
order by type ;";

            List<EvaluationDB> list = _orgUnitOfWork.Query<EvaluationDB>(sql, dy).ToList();
            if (list == null || list.Count == 0) return dto = null;
            dto.Id = list[0].Id;
            dto.UserId = request.UserId;
            dto.NickName = request.NickName;
            dto.Mobile = request.Mobile;
            dto.AuditStatus = list[0].AuditStatus;
            dto.HasVideo = list[0].HasVideo;
            dto.IsPlainText = list[0].IsPlainText;
            dto.Title = list[0].Title;
            dto.ModifyCount = list[0].ModifyCount ?? 0;
            dto.Video = list[0].Video;
            dto.VideoCover = list[0].VideoCover;
            List<string> Pictures = new List<string>();
            string Content = "";
            foreach (var e in list)
            {
                Pictures.AddRange(JsonSerializationHelper.JSONToObject<List<string>>(e.Pictures));
                Content += e.Content;
            }
            dto.Pictures = JsonSerializationHelper.Serialize(Pictures);
            dto.Content = Content;
            return dto;

            #region 检查SPU是否已有种草奖励记录
            string spuRecordSql = $@"
select count(*) from Evaluation as e
left join EvaluationBind as eb on e.id=eb.evaluationid
where auditstatus=1 and userid=@UserID and courseid in (
select courseid from Evaluation as e
left join EvaluationBind as eb on e.id=eb.evaluationid
where e.id=@Id and eb.IsValid=1)
";
            dto.SpuRecordCount = await _orgUnitOfWork.QueryFirstOrDefaultAsync<int>(spuRecordSql, new {dto.Id, dto.UserId });
            #endregion
            #region 标题或正文相同
            string sameCountSql = $@" 
select SUM(num)  from (
select count(1) as num from [dbo].[EvaluationItem] where IsValid=1  and content=@content and evaluationid<>@Id
union
select count(1) as num from [dbo].[Evaluation]  where IsValid=1  and title=@title and id<>@Id
)TT ;";
            dy.Set("content", dto.Content);
            dy.Set("title", dto.Title);
            if (_orgUnitOfWork.Query<int>(sameCountSql,dy).FirstOrDefault() > 0)
                dto.IsSame_TitleOrContent = true;
            #endregion

            string chanceSql = $@"SELECT member_id as userID,Convert(int,COUNT(member_id)*3*0.2) as TenYuanTotalChance,
(select count(*) from EvaluationReward as er
left join [Order] as o on o.id=er.OrderId
where er.UserId = sih.member_id and er.Used=1 and er.ModifyDateTime > '2021/11/1' and o.paymenttime>'2021/11/1') as TenYuanUsedChance
FROM sign_in_history as sih
WHERE bu_no='SHUANG11_ACTIVITY' 
and member_id in (select top 1000 member_id from sign_in where bu_no='SHUANG11_ACTIVITY' order by CreateTime)
and member_id=@UserID
and blocked=0 GROUP BY member_id";
            var chanceData = await _orgUnitOfWork.QueryFirstOrDefaultAsync<EvaluationDto>(chanceSql, new { dto.UserId });
            if (chanceData != null)
            {
                dto.TenYuanTotalChance = chanceData.TenYuanTotalChance;
                dto.TenYuanUsedChance = chanceData.TenYuanUsedChance;
                dto.TenYuanRemainChance = dto.TenYuanTotalChance - dto.TenYuanUsedChance;
            }
            if (dto.TenYuanRemainChance <= 0 || dto.SpuRecordCount>0)
            {
                 return dto;
            }
            if (dto.AuditStatus != 1)
            {
                #region 审核未通过 按钮展示
                var checkResult = await _mediator.Send(new EvltRewardCheckPassCommand() { EvltId = dto.Id, UserId = dto.UserId });
                for (var __ = checkResult != null; __; __ = !__)
                {
                    dto.IsShowPassBtn = true;
                    dto._EvaluationReward = checkResult;
                    if (checkResult.OrderId == null) break;
                    sql = "select Paymenttime from [order] where id=@OrderId";
                    dto.OrderPayTime = await _orgUnitOfWork.QueryFirstOrDefaultAsync<DateTime?>(sql, new { checkResult.OrderId });
                    //暂时屏蔽11月种草审核
                    if (dto.OrderPayTime < DateTime.Parse("2021/11/1") || dto.OrderPayTime> DateTime.Parse("2021/11/22") || dto.ModifyCount > 2)
                    {
                        dto.IsShowPassBtn = false;
                    }
                }
                #endregion
            }
            else
            {
                // 审核通过

                sql = $@"
select er.*,o.paymenttime from EvaluationReward er
join [Order] o on er.orderid=o.id and o.type>={OrderType.BuyCourseByWx.ToInt()} and o.IsValid=1 
where er.evaluationid=@Id
";
                var x = (await _orgUnitOfWork.QueryAsync<EvaluationReward, DateTime?, (EvaluationReward, DateTime?)>(sql,
                    splitOn: "paymenttime",
                    map: (er, time) => (er, time),
                    param: new { dto.Id })
                    ).FirstOrDefault();

                dto._EvaluationReward = x.Item1;
                dto.OrderPayTime = x.Item2;
            }

            if (dto != null)
            {
                //ViewBag.Title = result.Title;
                if (dto.IsOfficial)//官方
                {
                    if (!string.IsNullOrEmpty(dto.Content.Replace("<br/>", "  ")))
                    {
                        var arrStr = dto.Content.Split("<br/>");
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < arrStr.Length; i++)
                        {
                            sb.AppendLine("       " + HtmlHelper.NoHTML(arrStr[i]));
                            sb.AppendLine();
                        }

                        dto.Content = sb.ToString();
                        dto.Row = Math.Ceiling(sb.Length * 1.0 / 30.0);
                    }
                }
                else
                {
                    dto.Row = Math.Ceiling(dto.Content.Length * 1.0 / 30.0);
                }
            }


            return dto;
        }
    }
}
