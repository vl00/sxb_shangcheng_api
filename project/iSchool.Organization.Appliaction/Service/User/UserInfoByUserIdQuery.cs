using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.User
{
    /// <summary>
    /// 用户信息--我的
    /// </summary>
    public class UserInfoByUserIdQuery:IRequest<UserInfoByUserIdResponse>
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }
    }
}
