using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 发送微信模板消息to user
    /// </summary>
    public class SendWxTemplateMsgCmd : IRequest<SendWxTemplateMsgCmdResult>
    {
        /// <summary>
        /// 用户id. 设置了此参数后, 可以不设置WechatTemplateSendCmd里的openid
        /// </summary>
        public Guid UserId { get; set; }

        public WechatTemplateSendCmd WechatTemplateSendCmd { get; set; } = default!;
    }

#nullable disable
}
