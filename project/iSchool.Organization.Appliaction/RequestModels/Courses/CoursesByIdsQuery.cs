using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 学校--根据id集合，查询课程列表
    /// </summary>
    public class CoursesByIdsQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程Id集合
        /// </summary>
        public List<Guid> CourseIds { get; set; }
        public int IncludeGoodThing { get; set; }


    }
}
