using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 查询订单s的产品s
    /// </summary>
    public class OrderProdsByOrderIdsQuery : IRequest<OrderProdsByOrderIdsQryResult>
    {
        /// <inheritdoc cref="OrderProdsByOrderIdsQuery"/>
        public OrderProdsByOrderIdsQuery() { }

        public Guid[]? OrderIds { get; set; }

        /// <summary>
        /// 如不为null,优先使用
        /// </summary>
        public (Guid OrderId, OrderType OrderType)[]? Orders { get; set; }
    }

#nullable disable
}
