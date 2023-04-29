using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 学校--课程列表请求model
    /// </summary>
    public class CoursessByIDsQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程ID集合
        /// </summary>
        public List<Guid> CourseIds { get; set; }
    }


    

}
