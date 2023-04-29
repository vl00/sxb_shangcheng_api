using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Course
{
    /// <summary>
    /// 期待上线
    /// </summary>
    public class ExpectOnlineQurey : IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        public IUserInfo UserInfo { get; set; }

        /// <summary>
        /// ApiUrl
        /// </summary>
        public string ApiUrl { get; set; }
        /// <summary>
        /// 类型，0 期待未认证课程 1 购买认证课程
        /// </summary>
        public int Type { get; set; } = 0;
    }
}
