using iSchool.Organization.Appliaction.ResponseModels.Orders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Orders
{
    /// <summary>
    /// 根据订单Id获取订单详情[后台]
    /// </summary>
    public class OrderDetailsByOrdIdQuery:IRequest<List<OrderDetailsDto>>
    {
        /// <summary>
        /// 订单id
        /// </summary>
        public Guid OrderId { get; set; }
    }
}
