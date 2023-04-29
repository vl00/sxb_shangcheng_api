using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 兑换课程请求实体Model
    /// </summary>
    public class ExchangeCourseRequest
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid OrgId { get; set; }

        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        public string VerificationCode { get; set; }

    }
}
