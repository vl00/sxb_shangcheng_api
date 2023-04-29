using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.EvaluationCrawler
{

    /// <summary>
    /// 查询抓取评测列表
    /// </summary>
    public class CaptureEvaluationListQueryHandler : IRequestHandler<CaptureEvaluationListQuery, CrawlerListDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public CaptureEvaluationListQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public Task<CrawlerListDto> Handle(CaptureEvaluationListQuery request, CancellationToken cancellationToken)
        {

            var dy = new DynamicParameters()
            .Set("SkipCount", (request.PageIndex - 1) * request.PageSize)
            .Set("status", CaptureEvalStatusEnum.Published);
            
            string where = "";

            //类型
            if (request.Type != null)
            {
                if (Enum.IsDefined(typeof(GrabTypeEnum), request.Type))
                {
                    where += " and [type]=@Type  ";
                    dy.Add("@Type", request.Type);
                }
            }
            //时间
            if (request.StartTime != null && request.EndTime!=null)
            {
                where += " and [CreateTime] between @StartTime and @EndTime  ";
                dy.Add("@StartTime", request.StartTime);

                DateTime etime = ((DateTime)request.EndTime).AddHours(23).AddMinutes(59).AddSeconds(59);
                dy.Add("@EndTime", etime);
            }

            string sql = $@" 
                            select top {request.PageSize} * from 
                            (
                            select ROW_NUMBER() over (order by [CreateTime] desc ) as rownum , [id], [title], [type], [status], [content], [orgid], [courseid], [source]
                            , [specialid], [url], [cycle], [price], [age], [mode], [comments], [CreateTime]
                            from [dbo].[EvaluationCrawler] 
                            where [IsValid]=1 and status<>@status  {where}   
                            )TT where rownum>@SkipCount  order by rownum 
                        ;";
            string pageSql = $@" 
                               select COUNT(1) AS pagecount,{request.PageIndex} AS PageIndex,{ request.PageSize} AS PageSize
                               from [dbo].[EvaluationCrawler] 
                               where [IsValid]=1  and status<>@status  {where}                   
                             ;";
            var data = _orgUnitOfWork.DbConnection.Query<CrawlerListDto>(pageSql, dy).FirstOrDefault();
            data.list = new List<CrawlerItem>();
            data.list = _orgUnitOfWork.DbConnection.Query<CrawlerItem>(sql, dy).ToList();
            return Task.FromResult(data);
        }
    }
}
