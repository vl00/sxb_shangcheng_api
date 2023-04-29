using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Course
{
    public class CourseBookCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        public string VerifyCode { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 备注。昵称
        /// </summary>

        public string Remark { get; set; }


    }
    public class CourseBookRequest
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        public string VerifyCode { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 备注。昵称
        /// </summary>

        public string Remark { get; set; }

    }
}
