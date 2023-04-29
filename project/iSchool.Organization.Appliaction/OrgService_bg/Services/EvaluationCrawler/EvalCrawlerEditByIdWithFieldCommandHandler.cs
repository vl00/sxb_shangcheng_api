using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace iSchool.Organization.Appliaction.Service.EvaluationCrawler
{
   
    /// <summary>
    /// 抓取评测通过编辑方法
    /// </summary>
    public class EditOrgByIdWithFieldCommandHandler : IRequestHandler<EvalCrawlerEditByIdWithFieldCommand, ResponseResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public EditOrgByIdWithFieldCommandHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
        }
        public Task<ResponseResult> Handle(EvalCrawlerEditByIdWithFieldCommand request, CancellationToken cancellationToken)
        {
            request.Parameters.Add("@Id", request.Id);
            if (!string.IsNullOrEmpty(request.UpdateSql))
            {
                string updateSql = $@" update [dbo].[EvaluationCrawler] set {string.Join(',', request.UpdateSql)} where id=@Id;";
                var count = _orgUnitOfWork.DbConnection.Execute(updateSql, request.Parameters);
                if (count == 1)
                {
                    return Task.FromResult(ResponseResult.Success("操作成功"));
                }
                else
                {
                    return Task.FromResult(ResponseResult.Failed("操作失败"));
                }
            }
            else
            {
                return Task.FromResult(ResponseResult.Failed("操作失败"));
            }
        }
    }
}
