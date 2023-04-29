using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// call退款api
    /// </summary>
    public class RefundCmd : IRequest<RefundCmdResult>
    {
        /// <summary>预支付订单ID</summary>
        public Guid AdvanceOrderId { get; set; }

        /// <summary>子单号Id</summary>
        public Guid? OrderId { get; set; }

        /// <summary>子订单单个sku id</summary>
        public Guid? OrderDetailId { get; set; }
        /// <summary>sku Id</summary>
        public Guid? ProductId { get; set; }

        /// <summary>退款金额--单位元</summary>
        public decimal RefundAmount { get; set; }
        /// <summary>退款说明</summary>
        public string Remark { get; set; } = default!;
        public int System => 2;

        /// <summary>
        /// 退款类型 <br/>
        /// 1 = All(全部), 2 = ChildOrder(子单), 3 = ProductOrder(子单里面单个商品), 4 = 运费
        /// </summary>
        public int RefundType { get; set; }

        /// <summary>退sku时, 退的数量</summary>
        [Obsolete] public int? RefundProductNum { get; set; }

        /// <summary>
        /// 退款的价格，数量.(因存在同一个商品不同价格问题。前端决定退具体哪个)
        /// </summary>
        public IEnumerable<ProductInfo>? RefundProductInfo { get; set; }
        public class ProductInfo
        {
            /// <summary>退sku时, 退的数量</summary>
            public int RefundProductNum { get; set; }
            /// <summary>单个商品退的金额</summary>
            public decimal RefundProductPrice { get; set; }
            /// <summary>
            /// 退款金额
            /// </summary>
            public decimal Amount { get; set; }
        }


        public object? _others { get; set; }
    }

#nullable disable
}
