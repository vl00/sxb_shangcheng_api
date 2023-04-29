using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Special;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 查询机构列表
    /// </summary>
    public class QueryEvltsHandler : IRequestHandler<QueryEvlts, PagedList<SpecialEvlts>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public QueryEvltsHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }

        public Task<PagedList<SpecialEvlts>> Handle(QueryEvlts request, CancellationToken cancellationToken)
        {

            var dy = new DynamicParameters();
            dy.Add("@skipCount", (request.PageIndex-1)*request.PageSize);
            dy.Add("@pageSize", request.PageSize);
            string where ="";
            
            //科目
            if(request.Subject!=null && Enum.IsDefined(typeof(SubjectEnum), request.Subject))
            {
                where += " and (eb.subject=@subject or c.subject=@subject) ";
                dy.Set("subject", request.Subject);
            }

            //标题
            if (!string.IsNullOrEmpty(request.Title))
            {
                where += "  and contains(e.title,@title)  ";
                dy.Set("title", '"'+request.Title+'"');
            }

            //作者Id
            if(request.UserId!=null && request.UserId != default)
            {
                where += " and e.UserId=@UserId ";
                dy.Set("UserId", request.UserId);
            }
           
            string listSql = $@" 
                                SELECT  ROW_NUMBER()over(order by id desc) as rownum,id,title,CreateTime,specialid,UserId
                                ,case when specialid='{request.SpecialId}' then 'true' else 'false' end as ischeck from 
                                (
                                SELECT distinct e.id,e.title,CONVERT(VARCHAR(10),e.CreateTime,120) as CreateTime,spb.specialid,e.UserId
                                FROM [dbo].[Evaluation] e 
                                left join [dbo].[SpecialBind] spb on e.id=spb.evaluationid and spb.IsValid=1 
                                left join [dbo].[EvaluationBind] eb on e.id=eb.evaluationid and eb.IsValid=1
                                left join [dbo].[Course] c on eb.courseid=c.id and c.IsValid=1
                                where e.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()}  {where} 
                                )TT 
                                order by rownum OFFSET @skipCount ROWS FETCH NEXT @pageSize ROWS ONLY
                        ;";
            string countSql = $@" 
                               SELECT count(distinct e.id)
                                FROM [dbo].[Evaluation] e left join [dbo].[SpecialBind] spb on e.id=spb.evaluationid and spb.IsValid=1 
                                left join [dbo].[EvaluationBind] eb on e.id=eb.evaluationid and eb.IsValid=1
                                left join [dbo].[Course] c on eb.courseid=c.id and c.IsValid=1
                                where e.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()}  {where} 
                             ;";
            var totalItemCount = _orgUnitOfWork.DbConnection.Query<int>(countSql,dy).FirstOrDefault();
            var data = _orgUnitOfWork.DbConnection.Query<SpecialEvlts>(listSql, dy).ToPagedList(request.PageSize,request.PageIndex, totalItemCount);
            return Task.FromResult(data);
        }
    }
}
