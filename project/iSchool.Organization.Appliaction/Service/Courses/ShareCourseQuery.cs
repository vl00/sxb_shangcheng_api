using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Course
{
    public class ShareCourseQuery:IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public string Cnl { get; set; }

        public IUserInfo UserInfo { get; set; }

        /// <summary>
        /// 分销上一级user id或user code <br/>
        /// 没有传 null
        /// </summary>
        public string FxHeaducode { get; set; }
    }

    public class ShareCourseRequest 
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public string Cnl { get; set; }

        /// <summary>
        /// 分销上一级user id或user code <br/>
        /// 没有传 null
        /// </summary>
        public string FxHeaducode { get; set; }
    }

}
