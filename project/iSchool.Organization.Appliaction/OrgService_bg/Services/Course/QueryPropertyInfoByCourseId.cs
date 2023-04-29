using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 机构后台--获取课程属性-选项信息
    /// </summary>
    public class QueryPropertyInfoByCourseId : IRequest<List<PropertyAndItems>>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }
    }
}
