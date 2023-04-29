using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class OrderOrderRefundsByOrderIdsQuery : IRequest<IEnumerable<OrderRefundsDto>>
    {
        /// <summary>
        /// 订单ids
        /// </summary>
        public Guid[] OrderIds { get; set; }
    }
}
