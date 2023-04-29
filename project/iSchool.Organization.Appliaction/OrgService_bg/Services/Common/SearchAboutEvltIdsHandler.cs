using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.OrgService_bg.Common;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
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

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    /// <summary>
    /// 关于评测的其他Id(专题、机构、课程)
    /// </summary>
    public class SearchAboutEvltIdsHandler:IRequestHandler<SearchAboutEvltIds, AboutEvltIds>
    {
        OrgUnitOfWork _orgUnitOfWork;
        CSRedisClient _redisClient;

        public SearchAboutEvltIdsHandler(IOrgUnitOfWork unitOfWork, CSRedisClient redisClient)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _redisClient = redisClient;
        }
        public Task<AboutEvltIds> Handle(SearchAboutEvltIds request, CancellationToken cancellationToken)
        {


            string sql = @$" select sp.specialid,eb.orgid,eb.courseid from  [dbo].[EvaluationBind] eb
                            left join[dbo].[SpecialBind] sp on eb.evaluationid = sp.evaluationid and sp.IsValid = 1
                            where eb.IsValid = 1 and eb.evaluationid = @EvltId ";

            var data = _orgUnitOfWork.DbConnection.Query<AboutEvltIds>(sql,new DynamicParameters().Set("EvltId", request.EvltId)).FirstOrDefault();
           
            return Task.FromResult(data);

        }
    }
}
