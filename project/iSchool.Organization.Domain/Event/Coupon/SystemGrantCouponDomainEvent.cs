using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.Event.Coupon
{
    public class SystemGrantCouponDomainEvent: INotification
    {
        public CouponInfo CouponInfo { get;  }
        public CouponReceive CouponReceive { get;  }

        public SystemGrantCouponDomainEvent(CouponInfo couponInfo,CouponReceive couponReceive)
        {
            this.CouponInfo = couponInfo;
            this.CouponReceive = couponReceive;
                
        }




    }
}
