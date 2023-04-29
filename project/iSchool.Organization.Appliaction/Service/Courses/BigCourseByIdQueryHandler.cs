using CSRedis;
using Dapper;
using iSchool.Domain;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 体验课关联的大课详情
    /// </summary>
    public class BigCourseByIdQueryHandler : IRequestHandler<BigCourseByIdQuery, List<BigCourseResponse>>
    {
    
        OrgUnitOfWork _orgUnitOfWork;
       
        public BigCourseByIdQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient cSRedis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;  
        }
        
        public async Task<List<BigCourseResponse>> Handle(BigCourseByIdQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            string sql = $@"
select bc.id,bc.Title,bc.Price,bc.CashbackType,bc.CashbackValue,bc.PJCashbackType,bc.PJCashbackValue,bc.IsBonusRate,bc.HeadFxUserExclusiveType,bc.HeadFxUserExclusiveValue

,case bc.IsBonusRate when 1 then 
cast(ROUND((case bc.CashbackType when 1 then   bc.CashbackValue*bc.Price*0.01 else bc.CashbackValue end ),2,1) as money)else null end as CashbackMoney

,case  when bc.PJCashbackValue>0 then 
cast(ROUND((case bc.PJCashbackType when 1 then   bc.PJCashbackValue*bc.Price*0.01 else bc.PJCashbackValue end ),2,1) as money)else null end as PJCashbackMoney


,case when bc.HeadFxUserExclusiveValue>0 then 
cast(ROUND((case bc.HeadFxUserExclusiveType when 1 then bc.HeadFxUserExclusiveValue*bc.Price*0.01 else bc.HeadFxUserExclusiveValue end ),2,1) as money)
else null end as HeadFxUserExclusiveMoney

from [dbo].[BigCourse] as bc 
where bc.IsValid=1 and bc.courseid=@courseid
;";
            var data = _orgUnitOfWork.Query<BigCourseResponse>(sql, new DynamicParameters().Set("courseid", request.CourseId))?.ToList();
            return data;

        }

    }


    


}
