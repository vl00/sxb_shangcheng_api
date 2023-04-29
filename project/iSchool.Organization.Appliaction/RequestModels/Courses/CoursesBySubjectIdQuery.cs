using iSchool.Organization.Appliaction.OrgService_bg;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Courses
{
    /// <summary>
    /// 根据科目Id，获取课程列表
    /// </summary>
    public class CoursesBySubjectIdQuery:IRequest<List<CourseSelectItem>>
    {
        /// <summary>
        /// 科目Id
        /// </summary>
        public int? SubjectId { get; set; }
    }
}
