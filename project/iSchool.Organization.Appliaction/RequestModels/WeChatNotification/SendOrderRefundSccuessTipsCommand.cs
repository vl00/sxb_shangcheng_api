using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.WeChatNotification
{
    /// <summary>
    /// 发送退款成功提示
    /// </summary>
    public class SendOrderRefundSccuessTipsCommand:IRequest
    {
        public Guid ToUserId { get; set; }

        public Guid OrderRefundId { get; set; }

        public RefundTypeEnum RefundType { get; set; }

    }
}
