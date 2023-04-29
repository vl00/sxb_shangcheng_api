using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using MediatR;

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 评论
    /// </summary>
    public class EvltCommentByEvltIdQueryHandler : IRequestHandler<EvltCommentByEvltIdQuery, PagedList<EvltCommentItem>>
    {
        OrgUnitOfWork _orgUnitOfWork;

        public EvltCommentByEvltIdQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }
        public Task<PagedList<EvltCommentItem>> Handle(EvltCommentByEvltIdQuery request, CancellationToken cancellationToken)
        {
            string sql = $@" 
                            select  top  {request.PageSize} * from (
                            SELECT  ROW_NUMBER() over(order by CreateTime desc) as rownum,  * FROM [Organization].[dbo].[EvaluationComment] where IsValid=1 and fromid is null  and  evaluationid=@evltId
                            )TT  Where rownum>@SkipCount order by rownum 
                        ;";
            string pageSql = $@" 
                              select COUNT(1) AS TotalItemCount,{request.PageIndex} AS CurrentPageIndex,{ request.PageSize} AS PageSize
                              FROM [Organization].[dbo].[EvaluationComment] where IsValid=1 and  evaluationid=@evltId  and fromid is null
                             ;";
            var dy = new DynamicParameters()
                .Set("evltId", request.EvltId)
                .Set("SkipCount", (request.PageIndex-1)*request.PageSize);
            var data = _orgUnitOfWork.DbConnection.Query<PagedList<EvltCommentItem>>(pageSql, dy).FirstOrDefault();
            data.CurrentPageItems = new List<EvltCommentItem>();
            data.CurrentPageItems = _orgUnitOfWork.DbConnection.Query<EvltCommentItem>(sql, dy).ToList();
            return Task.FromResult(data);
        }

    }
}
