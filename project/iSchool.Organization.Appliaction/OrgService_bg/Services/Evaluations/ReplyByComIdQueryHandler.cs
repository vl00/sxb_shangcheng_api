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
    /// 回复
    /// </summary>
    public class ReplyByComIdQueryHandler : IRequestHandler<ReplyByComIdQuery, List<EvltCommentItem>>
    {
        OrgUnitOfWork _orgUnitOfWork;

        public ReplyByComIdQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }
        public Task<List<EvltCommentItem>> Handle(ReplyByComIdQuery request, CancellationToken cancellationToken)
        {
            string sql = $@" SELECT  ROW_NUMBER() over(order by reply.CreateTime desc) as rownum,  reply.* FROM [dbo].[EvaluationComment] as reply 
                            left join [dbo].[EvaluationComment] as com on com.id=reply.fromid and com.IsValid=1
                            where reply.IsValid=1 and  reply.fromid=@ComId and reply.fromid is not null order by rownum  ;";

            var dy = new DynamicParameters()
                .Set("ComId", request.ComId);
               
            var data = _orgUnitOfWork.DbConnection.Query<EvltCommentItem>(sql, dy).ToList();
           
            return Task.FromResult(data);
        }

    }
}
