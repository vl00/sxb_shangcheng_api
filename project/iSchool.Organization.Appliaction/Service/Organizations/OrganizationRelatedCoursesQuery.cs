using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 机构--相关课程【查询规则待产品确定】
    /// </summary>
    public class OrganizationRelatedCoursesQuery:IRequest<List<RelatedCourses>>
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfo PageInfo { get; set; }
        ///// <summary>
        ///// 页码
        ///// </summary>
        //public int PageIndex { get; set; }

        ///// <summary>
        ///// 页大小
        ///// </summary>
        //public int PageSize { get; set; }

        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid OrganizationId { get; set; }
    }
}
