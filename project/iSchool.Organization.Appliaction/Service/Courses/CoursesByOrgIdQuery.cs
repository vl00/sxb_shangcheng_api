using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Course
{
    public class CoursesByOrgIdQuery:IRequest<ResponseResult>
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfo PageInfo { get; set; }

        ///// <summary>
        ///// 页码
        ///// </summary>
        //public int PageIndex { get; set; } = 1;

        ///// <summary>
        ///// 页大小
        ///// </summary>
        //public int PageSize { get; set; } = 20;

        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid OrgId { get; set; }

        ///// <summary>
        ///// 短Id
        ///// </summary>
        //public long No { get; set; }
    }

    

}
