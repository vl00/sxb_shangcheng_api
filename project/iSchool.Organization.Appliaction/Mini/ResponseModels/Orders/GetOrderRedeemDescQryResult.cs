using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable    

    /// <summary>
    /// 订单兑换码页面
    /// </summary>
    public class GetOrderRedeemDescQryResult
    {
        /// <summary>订单id</summary>
        public Guid OrderId { get; set; }
        /// <summary>订单号</summary>
        public string OrderNo { get; set; } = default!;
        /// <summary>订单状态</summary>
        public int OrderStatus { get; set; }
        
        /// <summary>订单创建时间</summary>
        public DateTime OrderCreateTime { get; set; }
        
        
        /// <summary>兑换码.可null.</summary>
        public string? RedeemCode { get; set; }
        /// <summary>兑换码链接url.可null.</summary>
        public string? RedeemUrl { get; set; }
        /// <summary>兑换码提示框内容.可null.</summary>
        public string? RedeemMsg { get; set; }
        /// <summary>跳转兑换链接.可null.</summary>
        public bool? RedeemIsRedirect { get; set; }

        /// <summary>
        /// 内容s.根据内容类型显示不同的内容.<br/>
        /// 具体内容参考 字段 `__apidoc_OrderProdItemDto.__apidoc_ProdType_{prodType}`
        /// </summary>
        public OrderProdItemDto[] Prods { get; set; } = default!;
#if DEBUG
        public Apidoc_OrderProdItemDto? __apidoc_OrderProdItemDto { get; set; } = null;
#endif

        /// <summary>购前须知</summary>
        public CourseNoticeItem[]? CourseNotices { get; set; }
        
    }

#nullable disable
}
