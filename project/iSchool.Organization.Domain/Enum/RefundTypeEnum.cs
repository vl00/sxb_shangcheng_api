using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    public enum RefundTypeEnum
    {
        /// <summary>
        /// 退款
        /// </summary>
        [Description("退款")]
        Refund = 1,

        /// <summary>
        /// 退货
        /// </summary>
        [Description("退货")]
        Return = 2,

        /// <summary>
        /// 极速退款
        /// </summary>
        [Description("极速退款")]
        FastRefund = 3,

        /// <summary>
        /// 后台退款
        /// </summary>
        [Description("后台退款")]
        BgRefund = 4

    }
}
