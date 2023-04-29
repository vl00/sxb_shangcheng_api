using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
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
    /// 抓取评测详情
    /// </summary>
    public class CaptureEvaluationDetailsQueryHandler : IRequestHandler<CaptureEvaluationDetailsQuery, CaptureEvaluationDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public CaptureEvaluationDetailsQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork =(OrgUnitOfWork)unitOfWork;
        }

        public Task<CaptureEvaluationDto> Handle(CaptureEvaluationDetailsQuery request, CancellationToken cancellationToken)
        {
            var dy = new DynamicParameters();
            dy.Add("@Id", request.Id);
            string sql = $@" select *  from  [dbo].[EvaluationCrawler] where IsValid=1 and id=@Id ;";
            var dto= _orgUnitOfWork.DbConnection.Query<CaptureEvaluationDto>(sql, dy).FirstOrDefault();
            return Task.FromResult(dto);
        }
    }
}
