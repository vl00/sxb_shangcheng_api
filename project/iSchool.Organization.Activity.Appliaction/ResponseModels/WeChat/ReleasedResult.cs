using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Activity.Appliaction.ResponseModels.WeChat
{
    /// <summary>
    /// 发布完成返回公众号信息
    /// </summary>
    public class ReleasedResult
    {
        /// <summary>
        /// 公众号二维码
        /// </summary>
        public string QRCode { get; set; }

        /// <summary>
        /// 活动Id
        /// </summary>
        public Guid ActivityId { get; set; }

        /// <summary>
        /// 活动首页Url
        /// </summary>
        public string ActivityHomePageUrl { get; set; }
    }
}
