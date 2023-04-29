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
    public class OrgsLablesByInfoQueryHandler : IRequestHandler<OrgsLablesByInfoQuery, ResponseResult>
    {
        OrgUnitOfWork unitOfWork;
        CSRedisClient cSRedis;
        const int time = 60 * 60;//cache timeout

        public OrgsLablesByInfoQueryHandler(IOrgUnitOfWork unitOfWork, CSRedisClient cSRedis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.cSRedis = cSRedis;
           
        }



        public async Task<ResponseResult> Handle(OrgsLablesByInfoQuery request, CancellationToken cancellationToken)
        {
            #region Where
            var dy = new DynamicParameters();
            string sqlWhere = "";

            //机构类型            
            if (request.Type != null)
            {
                if (!Enum.IsDefined(typeof(Domain.Enum.OrgCfyEnum), request.Type))
                {
                    return ResponseResult.Failed("机构类型不存在");
                }
                else
                {
                    dy.Add("@Type", request.Type);
                    sqlWhere += $"  and AA.[types]=@Type  ";
                }
            }
            if (Guid.Empty != request.OrgId)
            {
                dy.Add("@OrgId", request.OrgId);
                sqlWhere += $"  and o.id  = @OrgId ";
            }
            //机构名称
            if (!string.IsNullOrEmpty(request.OrgName))
            {
                dy.Add("@CourseName", request.OrgName);
                sqlWhere += $"  and o.[name]  like '%{request.OrgName}%'";
            }
            #endregion

            dy.Add("@PageIndex", request.PageInfo.PageIndex);
            dy.Add("@PageSize", request.PageInfo.PageSize);

            string sql = $@"select top {request.PageInfo.PageSize} *  from 
                            (                               
                                select ROW_NUMBER() over(order by t.id desc) rownumber,* from
                                (
                                  select distinct o.id,o.no, '' as Id_s, o.[name] as OrgName , o.logo,o.[Desc],o.Subdesc,
 (SELECT count(distinct evaluationid)   FROM [dbo].[EvaluationBind] eb 
left join [dbo].[Evaluation] e on eb.evaluationid=e.id and e.IsValid=1 
where eb.[IsValid]=1 and eb.orgid=o.id and e.status={EvaluationStatusEnum.Ok.ToInt()} ) as EvalCount,
(select count(1)  from [dbo].[Course] where IsValid=1 and status={CourseStatusEnum.Ok.ToInt()} and orgid=o.id)as CourseCount
from [dbo].[Organization] o 
left join (SELECT id, value AS [types] FROM [Organization]CROSS APPLY OPENJSON([types])) AA on o.id=AA.id
where o.IsValid=1  and o.status={OrganizationStatusEnum.Ok.ToInt()}
                                {sqlWhere}
                                )t
                            )TT where rownumber>(@PageSize*(@PageIndex-1))
                             ";
            string sqlPage = $@" 
                                select
                                COUNT(1) AS TotalCount ,{request.PageInfo.PageIndex} AS PageIndex,{request.PageInfo.PageSize} AS PageSize
                            from
                                (
                                     select distinct o.id, o.name, o.logo, o.authentication,o.no
                                from [dbo].[Organization] o 
                                left join (SELECT id, value AS [types] FROM [Organization]CROSS APPLY OPENJSON([types])) AA on o.id=AA.id
                                where o.IsValid=1  and o.status=1
                                {sqlWhere}
                                 )T1 
                                ;";
            var  data = new OrgsLablesResponse();            
            var orgs= unitOfWork.Query<OrgLable>(sql, dy).ToList();
            for (int i = 0; i < orgs.Count; i++)
            {
                orgs[i].Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(orgs[i].No));
            }
            data.ListLables = orgs;            
            data.PageInfo = new PageInfoResult();
            data.PageInfo = unitOfWork.Query<PageInfoResult>(sqlPage, dy).FirstOrDefault();         
            data.PageInfo.TotalPage = (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);

            return ResponseResult.Success(data);
        }

    }
}
