using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.Event.Coupon
{

    /// <summary>
    /// CouponReceive 即将过期事件
    /// </summary>
    public class CouponReceiveWillExpireDomainEvent : INotification
    {
        public CouponReceive CouponReceive { get; private set; }
        public CouponReceiveWillExpireDomainEvent(CouponReceive couponReceive)
        {
            CouponReceive = couponReceive;
        }
    }
}
