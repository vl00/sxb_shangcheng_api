using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Activity.Appliaction.Service.WeChat
{
    
    /// <summary>
    /// 关注回复
    /// </summary>
    public class ReplyCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 活动Id（用于校对是否是本次活动）
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// 用户微信OpenID
        /// </summary>
        public string OpenID { get; set; }

        /// <summary>
        /// 缓存Key
        /// </summary>
        public string CacheKey { get; set; }

    }
}
