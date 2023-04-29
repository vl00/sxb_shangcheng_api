using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 订单支付成功ok Event
    /// </summary>
    public class OrderPayedOkEvent : INotification
    {
        /// <inheritdoc cref="OrderPayedOkEvent"/>
        public OrderPayedOkEvent() { }

        /// <summary>预订单id</summary>
        public Guid OrderId { get; set; }
    }

#nullable disable
}
