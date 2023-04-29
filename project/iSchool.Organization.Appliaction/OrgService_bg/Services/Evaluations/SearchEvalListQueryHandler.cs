using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    /// 评测管理-列表
    /// </summary>
    public class SearchEvalListQueryHandler : IRequestHandler<SearchEvalListQuery, EvalListDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;
        IHttpClientFactory httpClientFactory;
        IMediator _mediator;
        IConfiguration _config;

        public SearchEvalListQueryHandler(IOrgUnitOfWork unitOfWork, IConfiguration config
            , CSRedisClient redisClient
            , IHttpClientFactory httpClientFactory
            , IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
            this.httpClientFactory = httpClientFactory;
            _mediator = mediator;
            _config = config;
        }

        public async Task<EvalListDto> Handle(SearchEvalListQuery request, CancellationToken cancellationToken)
        {
            var userLs = default(IList<UserInfoByNameOrMobileResponse>);

            var SkipCount = (request.PageIndex - 1) * request.PageSize;
            var dy = new DynamicParameters()
            .Set("SkipCount", SkipCount);

            string where = " ";
            #region where

            //是否加精
            if (request.Stick != null)
            {
                where += "  and  Stick=@Stick ";
                dy.Add("@Stick", (bool)request.Stick ? 1 : 0);
            }

            //是否纯文字图片
            if (request.IsPlaintext != null)
            {
                where += " and IsPlaintext=@IsPlaintext ";
                dy.Add("@IsPlaintext", (bool)request.IsPlaintext ? 1 : 0);
            }

            //时间
            if (request.StartTime != null && request.EndTime != null)//时间端查询
            {
                where += " and evlt.CreateTime between @StartTime and @EndTime  ";
                dy.Add("@StartTime", request.StartTime);

                DateTime etime = ((DateTime)request.EndTime).AddHours(23).AddMinutes(59).AddSeconds(59);
                dy.Add("@EndTime", etime);
            }
            else if (request.StartTime != null && request.EndTime == null)//大于等于最小时间
            {
                where += " and evlt.CreateTime >= @StartTime";
                dy.Add("@StartTime", request.StartTime);
            }
            else if (request.StartTime == null && request.EndTime != null)//小于等于最大时间
            {
                where += " and evlt.CreateTime <= @EndTime  ";
                dy.Add("@EndTime", request.EndTime);
            }
            //科目
            if (request.Subject != null)
            {
                if (Enum.IsDefined(typeof(SubjectEnum), request.Subject))
                {
                    where += $@"
and  evlt.id  in  (select distinct bing.evaluationid from EvaluationBind as bing 
left join [dbo].[Course] c on bing.courseid=c.id and c.IsValid=1
left join (SELECT id, value AS csubject FROM [Course]CROSS APPLY OPENJSON(Subjects)) SS on c.id=SS.id
where bing.IsValid=1 and SS.csubject=@Subject)   
";
                    dy.Add("@Subject", request.Subject);
                }
            }

            //关联课程
            if (request.CourseId != null && request.CourseId != default)
            {
                where += @"  and evlt.id  in ( select evaluationid from EvaluationBind where IsValid=1 and courseid=@CourseId )  ";
                dy.Set("CourseId", request.CourseId);
            }

            //是否官方评测
            if (request.IsOfficial != null)
            {
                where += "  and  IsOfficial=@IsOfficial ";
                dy.Add("@IsOfficial", (bool)request.IsOfficial ? 1 : 0);
            }
            #region 筛查条件
            if (!string.IsNullOrEmpty(request.SearchField) && !string.IsNullOrEmpty(request.SearchFieldValue))
            {
                Guid userId;
                if (request.SearchField == "title")//标题-模糊 evlt.title like '%{request.SearchFieldValue}%' or
                {
                    //evlt.title like '%{request.SearchFieldValue}%' or
                    where += $@" and (evlt.title like @title0 or  CONTAINS(evlt.title,@title )   )";
                    dy.Add("@title0", "%" + request.SearchFieldValue + "%");
                    dy.Add("@title", $"{'"' + request.SearchFieldValue + '"'}");
                }
                else if (request.SearchField == "userid" && Guid.TryParse(request.SearchFieldValue, out userId))//作者-精准
                {
                    where += "  and evlt.userid=@userid  ";
                    dy.Add("@userid", new Guid(request.SearchFieldValue));
                }
                else if (request.SearchField == "content")//正文-模糊 content like '%{request.SearchFieldValue}%'  or
                {
                    where += $@" and evlt.id in(
							select id from (
								select  distinct  eval.* from Evaluation  as eval left join EvaluationItem  as item on eval.id=item.evaluationid 
								where eval.IsValid=1 and ( CONTAINS(content,@content)  )   and item.IsValid=1
								order by CreateTime desc OFFSET {SkipCount} ROWS FETCH NEXT {request.PageSize} ROWS ONLY
								)I
							)  ";
                    dy.Add("@content", $"{'"' + request.SearchFieldValue + '"'}");
                }
            }
            #endregion

            // 关联主体
            if (!string.IsNullOrEmpty(request.RelatedBody))
            {
                where += @"
and exists(select 1 from EvaluationBind eb 
left join Organization o on eb.orgid=o.id
left join course c on eb.courseid=c.id
where eb.Evaluationid=evlt.id and eb.IsValid=1 and (c.title like @RelatedBody or o.name like @RelatedBody) )
";
                dy.Add("@RelatedBody", "%" + request.RelatedBody + "%");
            }

            //是否上架
            if (request.IsOnTheShelf != null)
            {
                where += "  and  evlt.status=@evltstatus ";
                dy.Add("@evltstatus", (bool)request.IsOnTheShelf ? (int)EvaluationStatusEnum.Ok : (int)EvaluationStatusEnum.Fail);
            }

            //审核状态
            if (request.AuditStatus != null && Enum.IsDefined(typeof(EvltAuditStatusEnum), request.AuditStatus))
            {
                where += "  and isnull(evlt.auditstatus,0)=@auditstatus ";
                dy.Add("@auditstatus", request.AuditStatus);
            }

            for (var __ = !request.Mobile.IsNullOrEmpty() || !request.UserName.IsNullOrEmpty(); __; __ = !__)
            {
                userLs = await _mediator.Send(new UserInfoByNameOrMobileQuery
                {
                    Mobile = request.Mobile,
                    Name = request.UserName,
                });

                if (userLs?.Count < 1)
                {
                    where += " and evlt.Userid is null ";
                    break;
                }

                where += " and evlt.Userid in @userids ";
                dy.Set("@userids", userLs.Select(_ => _.Id).ToArray());
            }

            #endregion

            #region old
            //            string sql = $@" 
            //select  top  {request.PageSize} * from (
            //select evlt.id as EvalId,bing.id as BId,ROW_NUMBER() over (order by evlt.CreateTime desc) as rownum,evlt.title,evlt.cover
            //,evlt.ViewCount,CommentCount,Stick,IsPlaintext,evlt.status,courseid,IsOfficial,UserId,likes,shamlikes,auditstatus,
            //case when courseid is not null then c.subject else  bing.subject end as subject,evlt.no 
            //,evlt.DownloadMaterialCount,c.subjects
            //,(select top 1 videoCover from dbo.EvaluationItem as evltitem where evltitem.IsValid=1 and evltitem.evaluationid=evlt.id   and evltitem.videoCover is not null ) as videoCover
            //,c.title as CourseTitle
            //from [dbo].[Evaluation] evlt 
            //left join [dbo].[EvaluationBind] bing on evlt.id=bing.evaluationid and bing.IsValid=1
            //left join [dbo].[Course] c on bing.courseid=c.id and c.IsValid=1
            //left join (SELECT id, value AS csubject FROM [Course]CROSS APPLY OPENJSON(Subjects)) SS on c.id=SS.id
            //where evlt.IsValid=1  {where}
            //                            )TT  Where rownum>@SkipCount order by rownum 
            //                        ;"; 
            #endregion
            string sql = $@" 
select  top  {request.PageSize} * from (
select ROW_NUMBER() over (order by evlt.CreateTime desc) as rownum, evlt.id as EvalId,evlt.title,evlt.cover,evlt.ViewCount,evlt.CommentCount
,evlt.Stick,evlt.IsPlaintext,evlt.status,evlt.IsOfficial,evlt.UserId,evlt.likes,evlt.shamlikes,evlt.auditstatus,evlt.Auditor as AuditorId
,evlt.no,evlt.DownloadMaterialCount
,(select top 1 videoCover from dbo.EvaluationItem as evltitem where evltitem.IsValid=1 and evltitem.evaluationid=evlt.id and evltitem.videoCover is not null ) as videoCover
,evlt.CreateTime
from [dbo].[Evaluation] evlt
where evlt.IsValid=1  {where}
)TT  Where rownum>@SkipCount order by rownum 
;";
            string pageSql = $@" 
select COUNT(1) AS pagecount,{request.PageIndex} AS PageIndex,{request.PageSize} AS PageSize
from [dbo].[Evaluation] evlt 
where evlt.IsValid=1  {where}                      
;";
            var data = _orgUnitOfWork.DbConnection.Query<EvalListDto>(pageSql, dy).FirstOrDefault();
            data.list = new List<EvalItem>();
            data.list = _orgUnitOfWork.DbConnection.Query<EvalItem>(sql, dy).ToList();

            if (data?.list?.Any() == true)
            {
                #region 调沈老板接口获取UV
                var nos = data.list.Select(_ => _.No).ToArray();
                using var httpClient = httpClientFactory.CreateClient(string.Empty);
                var getUrl = $"{_config["AppSettings:pv_url"]}/getevalutionuv";
                string bodyp = nos.ToJsonString();
                var result = await httpClient.PostAsync(getUrl, new StringContent(nos.ToJsonString(), Encoding.UTF8, "application/json"));
                result.EnsureSuccessStatusCode();
                var r = (await result.Content.ReadAsStringAsync()).ToObject<UVModel>(true);

                #endregion

                #region 相关科目、课程
                var listCourseInfo = _orgUnitOfWork.DbConnection.Query<RelatedBodyInfo>($@"
select distinct bing.evaluationid as evltId,c.title AS CourseTitle,c.id as courseId,o.id as OrgId,o.name as OrgName,
(case when c.id is not null then c.Subjects when o.id is not null then o.Subjects else null end)as Subjects
from EvaluationBind as bing 
left join [Course] c on bing.courseid=c.id and c.IsValid=1
left join [Organization] o on o.id=bing.orgid and o.IsValid=1
where bing.IsValid=1 and bing.evaluationid in @evalIds
                ;", new { evalIds = data.list.Select(_ => _.EvalId) });
                #endregion

                for (int i = 0; i < data.list.Count; i++)
                {
                    var key = data.list[i].No;
                    data.list[i].UV = r.data.ContainsKey(key) ? r.data[key] : 0;

                    #region 课程相关信息
                    var infos = listCourseInfo.Where(_ => _.EvltId == data.list[i].EvalId).ToList();

                    data.list[i].Subjects = string.Join(",", infos.Where(_ => !string.IsNullOrEmpty(_.Subjects))
                        .SelectMany(_ => _.Subjects.ToObject<int[]>())
                        .Select(_ => ((SubjectEnum)_).GetDesc()));

                    data.list[i].ListCourses = infos.Where(_ => _.CourseId != null).ToList();
                    data.list[i].ListOrgs = infos.Where(_ => _.CourseId == null && _.OrgId != null).ToList();
                    #endregion
                }

                #region 用户信息
                var notIns = new List<EvalItem>();
                foreach (var itm in data.list)
                {   
                    if (userLs == null || !userLs.TryGetOne(out var u, _ => _.Id == itm.UserId))
                    {
                        notIns.Add(itm);
                        continue;
                    }
                    itm.NickName = u.NickName;
                    itm.Mobile = u.Mobile;
                }
                if (notIns.Count > 0)
                {
                    var userInfos = await _mediator.Send(new UserInfosByAPICommand { UserIds = notIns.Select(_ => _.UserId) });
                    foreach (var itm in notIns)
                    {
                        if (userInfos == null) continue;
                        if (!userInfos.TryGetOne(out var u, _ => _.Id == itm.UserId)) continue;
                        itm.NickName = u.NickName;
                        itm.Mobile = u.Mobile;
                    }
                }
                #endregion

                // 审核人
                var ns = AdminInfoUtil.GetNames(data.list.Select(_ => _.AuditorId ?? default).Where(_ => _ != default).Distinct());
                data.list.ForEach(p => p.AuditorName = p.AuditorId != null && ns.TryGetValue(p.AuditorId.Value, out var name) ? name : null);
            }

            return data;
        }
    }

    public class UVModel
    {
        public Dictionary<string, int> data { get; set; }

    }

    public class RelatedBodyInfo
    {
        public Guid EvltId { get; set; }
        public string Subjects { get; set; }

        public string CourseTitle { get; set; }
        public Guid? CourseId { get; set; }

        public Guid? OrgId { get; set; }
        public string OrgName { get; set; }        
    }


}
