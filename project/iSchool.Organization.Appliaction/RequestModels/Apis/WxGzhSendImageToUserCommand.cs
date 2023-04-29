using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// wx公众号发送客服消息-图片给用户<br/>
    /// 返回错误信息
    /// </summary>
    public class WxGzhSendImageToUserCommand : IRequest<string?>
    {
        /// <summary>公众号AppName</summary>
        public string GzhAppName { get; set; } = default!;
        /// <summary>用户的openid</summary>
        public string OpenIdToUser { get; set; } = default!;
        /// <summary>图片media_id</summary>
        public string MediaId { get; set; } = default!;

        /// <summary>微信客服消息api</summary>
        public string CustomerServiceApiUrl { get; set; } = "https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token=";
    }

#nullable disable
}
