using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{
    public class CreateCouponReceiveCommand :IRequest<CouponReceive>
    {
        public Guid? CouponId { get; set; }

        public string Number { get; set; }

        public Guid UserId { get; set; }

        public CouponReceiveOriginType OriginType { get; set; } = CouponReceiveOriginType.SelfReceive;

        public string Remark { get;  set; }


    }
}
