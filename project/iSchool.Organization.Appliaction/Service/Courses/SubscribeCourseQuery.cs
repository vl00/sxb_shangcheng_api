using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 期待上线
    /// </summary>
    public class SubscribeCourseQuery:IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程短Id
        /// </summary>
        public long No { get; set; }

    }
}
