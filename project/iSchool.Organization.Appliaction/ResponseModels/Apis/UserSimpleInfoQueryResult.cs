using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 查询用户s基本信息结果
    /// </summary>
    public class UserSimpleInfoQueryResult
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Nickname { get; set; }    
        /// <summary>
        /// 头像
        /// </summary>
        public string HeadImgUrl { get; set; }
    }
}
