using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.WeChatNotification
{
    /// <summary>
    /// 发送提醒用户填写物流信息通知。
    /// </summary>
    public class SendInputFreightInfoTipsCommand :IRequest
    {
        public Guid  ToUserId { get; set; }

        public Guid OrderRefundId { get; set; }

    }
}
