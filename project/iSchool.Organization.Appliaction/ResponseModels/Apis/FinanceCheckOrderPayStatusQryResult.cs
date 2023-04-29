using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class FinanceCheckOrderPayStatusQryResult
    {
        /// <inheritdoc cref="FinanceCenterOrderStatus"/>
        public int OrderStatus { get; set; }
        /// <summary>
        /// SUCCESS：支付成功 REFUND：转入退款 NOTPAY：未支付 CLOSED：已关闭 REVOKED：已撤销（付款码支付） <br/>
        /// USERPAYING：用户支付中（付款码支付） PAYERROR：支付失败(其他原因，如银行返回失败)
        /// </summary>
        public string WechatTradeState { get; set; }
        /// <summary>支付成功时间</summary>
        public DateTime? PaySuccessTime { get; set; }
    }
}

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 支付平台de支付状态
    /// </summary>
    public enum FinanceCenterOrderStatus
    {
        /// <summary>待支付</summary>
        Wait = 1,
        /// <summary>进行中</summary>
        Process = 2,
        /// <summary>已完成</summary>
        Finish = 3,
        /// <summary>已取消</summary>
        Cancel = 4,
        /// <summary>已退款</summary>
        Refund = 5,
        /// <summary>支付成功</summary>
        PaySucess = 6,
        /// <summary>支付失败</summary>
        PayFaile = 7,
    }
}
