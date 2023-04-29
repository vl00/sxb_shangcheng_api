using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{

    /// <summary>
    /// 订单退款特殊原因
    /// </summary>
    public enum OrderRefundSpecialReason
    {
        /// <summary>
        /// 无
        /// </summary>
        [Description("无")]
        Nothing = 0,
        /// <summary>
        /// 缺货
        /// </summary>、
        [Description("供应商缺货")]
        LackGoods = 1,

        /// <summary>
        /// 用户拒收
        /// </summary>
        [Description("用户已拒收")]
        UserRefuse = 2,

        /// <summary>
        /// 其它
        /// </summary>
        [Description("其它")]
        Others = 3,

    }
}
