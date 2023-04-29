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
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Domain.Enum;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    public class OrgsByIDsQueryHandler : IRequestHandler<OrgsByIDsQuery, ResponseResult>
    {
        OrgUnitOfWork unitOfWork;

        public OrgsByIDsQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;            
          
        }

        public async Task<ResponseResult> Handle(OrgsByIDsQuery request, CancellationToken cancellationToken)
        {
            if(request.OrgIds.Any()==false)
                return ResponseResult.Failed("机构Id不能为空集");

            string sql = $@"
select distinct o.id, o.name, o.logo, o.authentication,o.no
from [dbo].[Organization] o 
where o.IsValid=1  and o.status={OrganizationStatusEnum.Ok.ToInt()} and o.id in ('{string.Join("','", request.OrgIds)}');";

            
            var result = unitOfWork.Query<OrgQueryResult>(sql, null).ToList();
            if (result?.Any()==true)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    result[i].No = UrlShortIdUtil.Long2Base32(Convert.ToInt64(result[i].No));
                }
                return ResponseResult.Success(result);
            }
            else
            {
                return ResponseResult.Success("暂无数据");
            }
           
        }

    }
}
