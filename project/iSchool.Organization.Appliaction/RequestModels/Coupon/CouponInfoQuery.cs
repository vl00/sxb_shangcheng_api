using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{
    public class CouponInfoQuery:IRequest<CouponInfo>
    {
        public Guid Id { get; set; }
    }
}
