using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.WeChatNotification
{
    public class SendCouponReceiveWillExpireTipsCommnad : IRequest
    {
        public Guid CouponId { get; private set; }
        public string CouponValue { get; private set; }

        public Guid ToUserId { get;private set; }

        public SendCouponReceiveWillExpireTipsCommnad(Guid toUserId,Guid copuponId, string couponVlaue)
        {
            CouponValue = couponVlaue;
            ToUserId = toUserId;
            CouponId = copuponId;
        }

    }
}
