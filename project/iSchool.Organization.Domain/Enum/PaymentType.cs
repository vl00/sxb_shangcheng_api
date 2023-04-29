using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 支付类型
    /// </summary>
    public enum PaymentType
    {
        /// <summary>
        /// 微信支付 (微信手机页h5jsapi)
        /// </summary>
        [Description("微信支付")]
        Wx = 1,

        /// <summary>微信小程序支付</summary>
        [Description("微信小程序支付")]
        Wx_MiniProgram = 2,

        /// <summary>普通手机h5页使用微信支付</summary>
        [Description("普通手机h5页使用微信支付")]
        Wx_InH5 = 3,

        /// <summary>百度app使用微信支付</summary>
        [Description("百度app使用微信支付")]
        Wx_InBaidu = 4,
    }
}
