using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{
    public class CouponInfoOfflineCommand:IRequest<bool>
    {
        public Guid Id  { get; set; }

    }
}
