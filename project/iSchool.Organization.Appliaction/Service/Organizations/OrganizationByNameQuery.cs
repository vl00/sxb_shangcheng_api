using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Organization
{

    /// <summary>
    /// 根据机构名称(品牌)，查询机构列表
    /// </summary>
    public class OrganizationByNameQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfo PageInfo { get; set; }       

        /// <summary>
        /// 机构名称(模糊查询)
        /// </summary>
        public string OrgName { get; set; }
    }
}
