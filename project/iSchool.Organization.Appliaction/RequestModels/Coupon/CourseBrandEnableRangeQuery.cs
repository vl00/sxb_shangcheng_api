using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{
    public class CourseBrandEnableRangeQuery:IRequest<IEnumerable<CourseBrandEnableRange>>
    {
        public string Text { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }

    }
}
