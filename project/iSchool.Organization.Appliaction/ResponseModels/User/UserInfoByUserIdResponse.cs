using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 用户信息--我的 返回实体Model
    /// </summary>
    public class UserInfoByUserIdResponse
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string ProFile { get; set; }

        /// <summary>
        /// 发布
        /// </summary>
        public int ReleaseCount { get; set; } = 0;

        /// <summary>
        /// 回复
        /// </summary>
        public int ReplyCount { get; set; } = 0;

        /// <summary>
        /// 关注
        /// </summary>
        public int FollowCount { get; set; } = 0;

        /// <summary>
        /// 点赞
        /// </summary>
        public int LikeCount { get; set; } = 0;

    }
}
