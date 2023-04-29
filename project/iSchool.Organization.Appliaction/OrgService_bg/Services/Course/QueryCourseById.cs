using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 机构后台--获取课程详情
    /// </summary>
    public class QueryCourseById : IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 是否忽略课程上下架状态（默认false,只查上架的；true：上下架都查）
        /// </summary>
        public bool IgnoreStatus { get; set; } = false;
    }
}
