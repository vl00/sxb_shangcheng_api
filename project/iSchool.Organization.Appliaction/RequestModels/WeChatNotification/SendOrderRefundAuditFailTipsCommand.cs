using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.WeChatNotification
{
    /// <summary>
    /// 发送提醒用户售后单审核失败信息息通知。
    /// </summary>
    public class SendOrderRefundAuditFailTipsCommand : IRequest
    {
        public Guid  ToUserId { get; set; }

        public Guid OrderRefundId { get; set; }

    }
}
