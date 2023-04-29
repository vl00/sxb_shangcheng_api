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
    /// 订单详情 (简易, 支付成功回调会使用)
    /// </summary>
    public class OrderDetailQuery : IRequest<OrderDetailQueryResult>
    {
        public Guid OrderId { get; set; }
        public string? OrderNo { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public Guid UserId { get; set; }
    }

    /// <summary>
    /// 根据预订单id查简易订单详情（已支付也是查预订单下的所有订单）
    /// </summary>
    public class OrderDetailSimQuery : IRequest<OrderDetailSimQryResult>
    {
        // 整个预订单
        public Guid AdvanceOrderId { get; set; }
        public string? AdvanceOrderNo { get; set; }

        // 单个订单
        public Guid OrderId { get; set; }
        public string? OrderNo { get; set; }

        public bool IgnoreCheckExpired { get; set; } = false;
        public bool UseReadConn { get; set; } = false;
    }

#nullable disable
}
