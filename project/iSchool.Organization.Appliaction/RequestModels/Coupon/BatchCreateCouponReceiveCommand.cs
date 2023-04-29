using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{
    public class BatchCreateCouponReceiveCommand : IRequest<ResponseResult>
    {
        public Guid CouponId { get; set; }

        public List<Guid> UserIds { get; set; }

        /// <summary>
        /// 发放人， 默认是系统 000000-00000-000000000000000000000
        /// </summary>
        public Guid SenderID { get; set; }
    }
}
