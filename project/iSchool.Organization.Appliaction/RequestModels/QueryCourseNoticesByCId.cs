using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using iSchool.Organization.Domain;
using MediatR;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 课程的购前须知
    /// </summary>
    public class QueryCourseNoticesByCId:IRequest<List<CourseNotices>>
    {
        public Guid CourseId { get; set; }
    }
}
