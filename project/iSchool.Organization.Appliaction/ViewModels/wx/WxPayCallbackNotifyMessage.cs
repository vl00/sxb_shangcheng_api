using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    public class WxPayCallbackNotifyMessage
    {
        public Guid OrderId { get; set; }
        public WxPayCallbackNotifyPayStatus PayStatus { get; set; }
        public DateTime AddTime { get; set; }
    }
}

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// wx支付后回调支付平台,支付平台给出的支付状态
    /// </summary>
    public enum WxPayCallbackNotifyPayStatus
    {
        /// <summary>待支付</summary>
        [Description("待支付")]
        InProcess = 0,
        /// <summary>成功</summary>
        [Description("成功")]
        Success = 1,
        /// <summary>失败</summary>
        [Description("失败")]
        Fail = 2
    }
}
