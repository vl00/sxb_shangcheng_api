using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 认领机构列表请求参数
    /// </summary>
    public class ClaimOrganizationsListQuery:IRequest<ClaimOrgListDto>
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }
    }
}
