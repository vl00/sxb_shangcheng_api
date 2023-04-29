using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{

    /// <summary>
    /// 批量发送优惠券即将过期事件
    /// </summary>
    public class SendWillExpireMsgNotifyCommand : IRequest
    {
        public Guid CouponReceiveId { get; set; }

    }
}
