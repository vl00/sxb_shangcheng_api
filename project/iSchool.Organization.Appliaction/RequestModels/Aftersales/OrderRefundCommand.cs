using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{

    /// <summary>
    /// 订单退款
    /// </summary>
    public class OrderRefundCommand: IRequest
    {

        public Guid AdvanceOrderId { get; set; }

        public Guid OrderId { get; set; }

        public Guid OrderDetailId { get; set; }

        public Guid ProductId { get; set; }

        public string Remark { get; set; }

        /// <summary>
        /// 退款价格表（一个OrderDetail 需要拆分价格来退款）
        /// unitPrice 支付单价，refundPrice 退款单价 （退款单价* 数量 <= 支付单价*数量）
        /// </summary>
        public IEnumerable<(decimal unitPrice,decimal refundAmount,int number)> RefundPrices { get; set; }


    }
}
