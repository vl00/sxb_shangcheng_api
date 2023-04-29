using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.Event
{

    /// <summary>
    /// 订单退款成功事件
    /// </summary>
    public class OrderRefundSuccessDomainEvent:INotification
    {
        /// <summary>
        /// 售后单ID
        /// </summary>
        public Guid OrderRefundId { get; set; }
    }
}
