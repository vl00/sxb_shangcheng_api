using CSRedis;
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

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 认领机构列表
    /// </summary>
    public class ClaimOrganizationsListQueryHandler : IRequestHandler<ClaimOrganizationsListQuery, ClaimOrgListDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        

        public ClaimOrganizationsListQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            
        }

        public Task<ClaimOrgListDto> Handle(ClaimOrganizationsListQuery request, CancellationToken cancellationToken)
        {
            var dy = new DynamicParameters();
            dy.Add("@skipCount", request.PageSize*(request.PageIndex-1));
            string sqlData = $@" select top {request.PageSize} * from (
                                 select ROW_NUMBER() over (order by org.id desc) as rownum,org.id as orgId,org.name as orgname,act.name,act.mobile,act.position,act.status ,act.id
                                 from [dbo].[Authentication] act left join  [dbo].[Organization] org on org.id=act.orgid and org.IsValid=1 
                                 --where act.IsValid=1 已拒绝的也需要显示
                                 )TT
                                 where rownum>@skipCount   order by rownum 
;";

            string sqlPageInfo = $@"select COUNT(1) AS pagecount ,{request.PageIndex} as PageIndex, {request.PageSize} as PageSize
                                 from [dbo].[Authentication] act left join  [dbo].[Organization] org on org.id=act.orgid and org.IsValid=1 ;";

            var response = _orgUnitOfWork.DbConnection.Query<ClaimOrgListDto>(sqlPageInfo).FirstOrDefault();
            var data = _orgUnitOfWork.DbConnection.Query<ClaimOrgItem>(sqlData, dy).ToList();
            response.list = new List<ClaimOrgItem>();
            response.list = data;

            return Task.FromResult(response);
        }
    }
}
