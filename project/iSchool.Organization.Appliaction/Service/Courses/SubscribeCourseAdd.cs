using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 订阅课程
    /// </summary>
    public class SubscribeCourseAdd:IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 用户微信OpenID
        /// </summary>
        public string OpenID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string  UserName { get; set; }

        /// <summary>
        /// 0 期待上线 1购买课程
        /// </summary>
        public int Type { get; set; }



    }
}
