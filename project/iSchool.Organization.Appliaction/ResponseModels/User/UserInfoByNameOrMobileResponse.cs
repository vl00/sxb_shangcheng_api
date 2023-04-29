using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class UserInfoByNameOrMobileResponse
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string HeadImgUrl { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string NickName { get; set; }
    }
}
