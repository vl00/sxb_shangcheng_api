using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{
    public class SetCouponEnableRangeCommand:IRequest
    {
        public CouponInfo CouponInfo { get; set; }
    }
}
