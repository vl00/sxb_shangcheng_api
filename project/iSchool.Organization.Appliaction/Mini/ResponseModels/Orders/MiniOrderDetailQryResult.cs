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
    /// mini 订单详情
    /// </summary>
    public partial class MiniOrderDetailQryResult
    {
        /// <summary>
        /// 预订单Id
        /// </summary>
        public Guid AdvanceOrderId { get; set; }
        /// <summary>订单id</summary>
        public Guid OrderId { get; set; }
        /// <summary>订单号</summary>
        public string OrderNo { get; set; } = default!;
        /// <summary>订单状态(中文)</summary>
        public string OrderStatusDesc { get; set; } = default!;
        /// <summary>订单状态</summary>
        public int OrderStatus { get; set; }
        /// <summary>
        /// 订单类型<br/>
        /// ```
        /// /// <summary>认证课程购买</summary>        
        /// CourseBuy = 1,        
        /// /// <summary>微信方式购买课程</summary>
        /// BuyCourseByWx = 2,
        /// ```
        /// </summary>
        public int OrderType { get; set; }
        /// <summary>订单总金额(实付)</summary>
        public decimal Paymoney { get; set; }

        /// <summary>支付方式.可null.</summary>
        public int? Paytype { get; set; }
        /// <summary>支付方式（中文）.可null.</summary>
        public string? PaytypeDesc { get; set; }

        /// <summary>订单创建时间</summary>
        public DateTime OrderCreateTime { get; set; }
        /// <summary>用户支付时间.可null.</summary>
        public DateTime? UserPayTime { get; set; }
        /// <summary>订单最后更新时间</summary>
        public DateTime OrderUpdateTime { get; set; }
        /// <summary>发货时间.可null.</summary>
        public DateTime? SendExpressTime { get; set; }

        /// <summary>收货地址dto</summary>
        public RecvAddressDto RecvAddressDto { get; set; } = default!;


        /// <summary>约课状态（中文）.可null.</summary>
        public string? BookingCourseStatusDesc { get; set; }
        /// <summary>约课状态（数值）.可null.</summary>
        public int? BookingCourseStatus { get; set; }


        /// <summary>兑换码.可null.</summary>
        public string? RedeemCode { get; set; }
        /// <summary>兑换码链接url.可null.</summary>
        public string? RedeemUrl { get; set; }
        /// <summary>兑换码提示框内容.可null.</summary>
        public string? RedeemMsg { get; set; }
        /// <summary>跳转兑换链接.可null.</summary>
        public bool? RedeemIsRedirect { get; set; }

        /// <summary>最新物流记录.可null.</summary>
        public string? LastExpressDesc { get; set; }
        /// <summary>最新物流时间.可null.</summary>
        public DateTime? LastExpressTime { get; set; }
        /// <summary>快递公司.可null.</summary>
        public string? ExpressCompanyName { get; set; }
        /// <summary>快递单号.可null.</summary>
        public string? ExpressNu { get; set; }


        /// <summary>
        /// 内容s.根据内容类型显示不同的内容.<br/>
        /// 具体内容参考 字段 `__apidoc_OrderProdItemDto.__apidoc_ProdType_{prodType}`
        /// </summary>
        public OrderProdItemDto[] Prods { get; set; } = default!;
#if DEBUG
        public Apidoc_OrderProdItemDto? __apidoc_OrderProdItemDto { get; set; } = null;
#endif
        public Guid UserId { get; set; }

        /// <summary>小助手二维码</summary>
        public string? Qrcode { get; set; }

        /// <summary>订单备注</summary>
        public string? Remark { get; set; }
    }

#nullable disable
}
