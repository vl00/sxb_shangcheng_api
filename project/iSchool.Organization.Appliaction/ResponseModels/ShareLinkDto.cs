using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 分享链接
    /// </summary>
    public class ShareLinkDto
    {
        /// <summary>
        /// base64二维码
        /// </summary>
        public string Base64QRCode { get; set; }
        /// <summary>
        /// 封面图
        /// </summary>
        public string Banner { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string MainTitle { get; set; }
        /// <summary>
        /// 副标题
        /// </summary>
        public string SubTitle { get; set; }
        /// <summary>
        /// 作者名字
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 作者头像
        /// </summary>
        public string UserHeadImg { get; set; }
    }
}
