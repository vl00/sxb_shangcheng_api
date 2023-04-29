using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable    

    /// <summary>
    /// 订单详情
    /// </summary>
    public partial class OrderDetailQueryResult
    {
        /// <summary>订单id</summary>
        public Guid OrderId { get; set; }
        /// <summary>订单号</summary>
        public string OrderNo { get; set; } = default!;
        /// <summary>订单状态(中文)</summary>
        public string OrderStatusDesc { get; set; } = default!;
        /// <summary>
        /// 订单状态
        /// </summary>
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
        /// <summary>总订单金额</summary>
        public decimal Paymoney { get; set; }
        /// <summary>订单金额(不含运费等商品总额)</summary>
        public decimal Paymoney0 { get; set; }

        /// <summary>订单创建时间</summary>
        public DateTime OrderCreateTime { get; set; }
        /// <summary>用户支付时间</summary>
        public DateTime? UserPayTime { get; set; }
        /// <summary>订单更新时间</summary>
        public DateTime OrderUpdateTime { get; set; }

        public string? Qrcode { get; set; }

        #region RecvAddressDto
        /// <summary>用户名字</summary>
        public string RecvUsername { get; set; } = default!;
        /// <summary>用户手机</summary>
        public string RecvMobile { get; set; } = default!;
        /// <summary>地址</summary>
        public string Address { get; set; } = default!;
        /// <summary>省份</summary>
        public string Province { get; set; } = default!;
        /// <summary>城市</summary>
        public string City { get; set; } = default!;
        /// <summary>地区</summary>
        public string Area { get; set; } = default!;
        #endregion

        /// <summary>年龄</summary>
        public string? Age { get; set; }

        /// <summary>
        /// 内容s.根据内容类型显示不同的内容.<br/>
        /// 具体内容参考 字段 `__apidoc_OrderProdItemDto.__apidoc_ProdType_{prodType}`
        /// </summary>
        public OrderProdItemDto[] Prods { get; set; } = default!;
#if DEBUG
        public Apidoc_OrderProdItemDto? __apidoc_OrderProdItemDto { get; set; } = null;
#endif

        /// <summary>购买人userid</summary>
        public Guid UserId { get; set; }
        /// <summary>上课电话</summary>
        public string? BeginClassMobile { get; set; } = default!;

        public Guid AdvanceOrderId { get; set; }
        public string AdvanceOrderNo { get; set; } = default!;


        /// <summary>
        /// 订单消耗积分
        /// </summary>
        public int? TotalPoints { get; set; }
    }

    public class OrderDetailSimQryResult
    {
        public Guid AdvanceOrderId { get; set; }
        public string AdvanceOrderNo { get; set; } = default!;

        /// <summary>没结果时为null</summary>
        public OrderDetailQueryResult[] Orders { get; set; } = default!;

        public Guid UserId => Orders?.FirstOrDefault()?.UserId ?? default;
        public OrderType OrderType => (OrderType)(Orders?.FirstOrDefault()?.OrderType ?? 0);
    }

#nullable disable
}
