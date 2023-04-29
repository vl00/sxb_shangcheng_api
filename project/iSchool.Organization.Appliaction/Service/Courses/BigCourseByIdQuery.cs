using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 体验课关联的大课列表请求实体
    /// </summary>
    public class BigCourseByIdQuery : IRequest<List<BigCourseResponse>>
    {
        /// <summary>
        /// 体验课Id
        /// </summary>
        public Guid CourseId { get; set; }

    }

    

}
