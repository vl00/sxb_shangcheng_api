using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.Event.Coupon
{
    public class CouponLoseEfficacyDomainEvent : INotification
    {
        public CouponInfo CouponInfo { get; set; }

        public CouponLoseEfficacyDomainEvent(CouponInfo couponInfo)
        {
            CouponInfo = couponInfo;
        }
    }
}
