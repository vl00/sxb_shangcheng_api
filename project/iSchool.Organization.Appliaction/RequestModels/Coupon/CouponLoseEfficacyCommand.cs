using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{
    public class CouponLoseEfficacyCommand : IRequest<bool>
    {
        /// <summary>
        /// CouponInfoID
        /// </summary>
        public Guid Id { get; set; }
    }
}
