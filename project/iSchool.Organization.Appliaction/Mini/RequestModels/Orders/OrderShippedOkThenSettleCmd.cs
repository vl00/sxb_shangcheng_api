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
    /// mini 确定收货后解锁佣金
    /// </summary>
    public class OrderShippedOkThenSettleCmd : IRequest<object>
    {
        /// <summary>订单Id</summary>
        public Guid OrderId { get; set; }
        public OrderDetailQueryResult? Order { get; set; }
        public bool IsFixUpTime { get; set; }
    }

#nullable disable
}
