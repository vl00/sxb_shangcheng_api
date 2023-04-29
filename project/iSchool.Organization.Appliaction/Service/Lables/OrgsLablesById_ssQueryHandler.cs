using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using Microsoft.Extensions.Configuration;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using iSchool.Organization.Appliaction.ResponseModels.Lables;
using iSchool.Organization.Domain.Enum;

namespace iSchool.Organization.Appliaction.Service.Lables
{
    /// <summary>
    /// 【短ID集合】机构卡片列表
    /// </summary>
    public class OrgsLablesById_ssQueryHandler : IRequestHandler<OrgsLablesById_ssQuery, ResponseResult>
    {
        OrgUnitOfWork unitOfWork;

        public OrgsLablesById_ssQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
           
        }



        public async Task<ResponseResult> Handle(OrgsLablesById_ssQuery request, CancellationToken cancellationToken)
        {
            #region Where
            var dy = new DynamicParameters();
            string sqlWhere = "";

            
            if (request.Id_ss.Any()==false)
            {
                return ResponseResult.Failed("Id集不允许为空集");
            }
            else
            {
                var nos = request.Id_ss.Select(_ => UrlShortIdUtil.Base322Long(_)).ToList();
                sqlWhere += $"  and o.no in ({string.Join(",", nos)}) ";
            }
           
            #endregion

           
            string sql = $@"select distinct o.id,o.no, '' as Id_s, o.[name] as OrgName , o.logo,o.[Desc],o.Subdesc,
 (SELECT count(distinct evaluationid)   FROM [dbo].[EvaluationBind] eb 
left join [dbo].[Evaluation] e on eb.evaluationid=e.id and e.IsValid=1 
where eb.[IsValid]=1 and eb.orgid=o.id and e.status={EvaluationStatusEnum.Ok.ToInt()} ) as EvalCount,
(select count(1)  from [dbo].[Course] where IsValid=1 and status={CourseStatusEnum.Ok.ToInt()} and orgid=o.id)as CourseCount
from [dbo].[Organization] o 
left join (SELECT id, value AS [types] FROM [Organization]CROSS APPLY OPENJSON([types])) AA on o.id=AA.id
where o.IsValid=1  and o.status={OrganizationStatusEnum.Ok.ToInt()}
                                {sqlWhere}
                             ";
                               
            var orgs= unitOfWork.Query<OrgLable>(sql, dy).ToList();
            if (orgs.Any() == false)
            {
                return ResponseResult.Success("暂无符合条件的数据");
            }
            for (int i = 0; i < orgs.Count; i++)
            {
                orgs[i].Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(orgs[i].No));
            }
           
            return ResponseResult.Success(orgs);
        }

    }
}
