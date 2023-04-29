using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.Event.Coupon
{

    public class UserReceiveCouponDomainEvent: INotification
    {
        public CouponReceive  CouponReceive { get; }


        public UserReceiveCouponDomainEvent(CouponReceive couponReceive)
        {
            this.CouponReceive = couponReceive;
        }

    }
}
