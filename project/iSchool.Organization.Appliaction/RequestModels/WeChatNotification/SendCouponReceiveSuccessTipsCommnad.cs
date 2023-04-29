using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.WeChatNotification
{
    public class SendCouponReceiveSuccessTipsCommnad : IRequest
    {
        public Guid CouponId { get; private set; }
        public string CouponValue { get; private set; }

        public Guid ToUserId { get;private set; }

        public DateTime? CouponExpireTime { get; set; }

        public SendCouponReceiveSuccessTipsCommnad(Guid toUserId,Guid copuponId, string couponVlaue,DateTime? couponExpireTime)
        {
            CouponValue = couponVlaue;
            ToUserId = toUserId;
            CouponId = copuponId;
            CouponExpireTime = couponExpireTime;
        }

    }
}
