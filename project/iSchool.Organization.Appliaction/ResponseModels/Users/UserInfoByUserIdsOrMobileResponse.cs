using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 用户信息 返回实体Model
    /// </summary>
    public class UserInfoByUserIdsOrMobileResponse
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 微信昵称
        /// </summary>
        public string WXNickName { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string Mobile { get; set; }
        
    }
}
