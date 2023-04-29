using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Wechat
{
    public class WechatTemplateSendCmd : IRequest<bool>
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public Guid OrderID { get; set; }

        /// <summary>
        /// 种草Id
        /// </summary>
        public Guid EvltId { get; set; }

        public string First { get; set; }
        public string KeyWord1 { get; set; }
        public string KeyWord2 { get; set; }
        public string KeyWord3 { get; set; }
        public string KeyWord4 { get; set; }
        public string Remark { get; set; }
        public WechatMessageType MsyType { get; set; }
        public string OpenId { get; set; }
        public string Href { get; set; }
        public string UserNick { get; set; }

        public Dictionary<string, object> Args { get; set; }
    }
}
