using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{

    /// <summary>
    /// 取消券使用
    /// </summary>
    public class CancelCouponCommand:IRequest
    {
        /// <summary>
        /// AdvanceId
        /// </summary>
        public Guid OrderId { get; set; }
    }
}
