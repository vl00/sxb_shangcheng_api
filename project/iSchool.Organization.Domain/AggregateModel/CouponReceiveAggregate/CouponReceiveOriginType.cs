using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate
{
    /// <summary>
    /// 优惠券领取来源类型
    /// </summary>
    public enum CouponReceiveOriginType
    {
        /// <summary>
        /// 来源于系统发放
        /// </summary>
        FromSystem =  1,

        /// <summary>
        /// 自领取
        /// </summary>
        SelfReceive = 2,

    }
}
