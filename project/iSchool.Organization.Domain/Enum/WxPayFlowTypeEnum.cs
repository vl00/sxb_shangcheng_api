using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 微信支付流程类型 <br/>
    /// 决定wx支付走的方式(是原支付还是新的跳转支付)
    /// </summary>
    public enum WxPayFlowTypeEnum
    {
        /// <summary>原来的支付</summary>
        [Description("原支付")]
        Tyl = 1,
        /// <summary>下单后跳转另一小程序支付</summary>
        [Description("跳转支付")]
        Ty2 = 2,
    }
}
