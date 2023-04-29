using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace iSchool.Organization.Domain.Event.Order
{
    /// <summary>
    /// 订单详情流转到退款状态领域事件
    /// </summary>
    public class OrderDetailTransferToRefundStateDomainEvent : INotification
    {
        public Guid OrderDetailId { get; set; }
        public OrderDetailTransferToRefundStateDomainEvent(Guid orderDetailId)
        {
            this.OrderDetailId = orderDetailId;
        }

    }
}
