using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 订单list
    /// </summary>
    [Obsolete]
    public class OrderPglistQueryResult
    {
        public NameCodeDto<int>[] OrderStatusArr { get; set; } = default!;
        public PagedList<OrderItemDto> PageInfo { get; set; } = default!;
    }

    /// <summary>订单 list item</summary>
    public class OrderItemDto
    {
        public Guid? LogisticsId { get; set; }
        /// <summary>订单id</summary>
        public Guid OrderId { get; set; }
        /// <summary>
        ///订单编号，核心参数 
        /// </summary>
        public string AdvanceOrderNo { get; set; } = default!;
        public Guid AdvanceOrderId { get; set; }
        /// <summary>订单号</summary>
        public string OrderNo { get; set; } = default!;
        /// <summary>订单状态(中文)</summary>
        public string OrderStatusDesc { get; set; } = default!;
        /// <summary>订单状态</summary>
        public int OrderStatus { get; set; }
        /// <summary>订单类型</summary>
        public int OrderType { get; set; }
        /// <summary>订单状态类型</summary>
        public int OrderStatusType { get; set; }//1待付款，2其他 
        /// <summary>订单总金额</summary>
        public decimal Paymoney { get; set; }
        /// <summary>
        /// 消耗总积分
        /// </summary>
        public int? TotalPoints { get; set; }

        /// <summary>
        /// 优惠总金额
        /// </summary>
        public decimal CouponAmount
        {
            get
            {
                if (this.Prods != null && this.Prods.Any())
                    return this.Prods.Sum(p => p.CouponAmount);
                else return 0;

            }
        }
        /// <summary>订单创建时间</summary>
        public DateTime CreateTime { get; set; }

        /// <summary>订单更新时间. (貌似这个字段没什么用)</summary>
        public DateTime? OrderUpdateTime { get; set; }

        /// <summary>
        /// 内容s.根据内容类型显示不同的内容.<br/>
        /// 内容类型参考api-订单详情 `/api/order/v2/detail/{id}` 字段 `__apidoc_OrderProdItemDto.__apidoc_ProdType_{prodType}`
        /// </summary>
        public OrderProdItemDto[] Prods { get; set; } = default!;


        /// <summary>退款时间</summary>
        public DateTime? RefundTime { get; set; }
        /// <summary>
        /// 退货审核时间
        /// </summary>
        public string? StepOneTime { get; set; }

        /// <summary>
        /// 退货申请时间
        /// </summary>
        public DateTime? OrderRefundApplyTime { get; set; }
        /// <summary>
        /// 退款类型
        /// </summary>
        public byte RefundType { get; set; }

        /// <summary>
        /// 退款状态
        /// </summary>
        public byte RefundStatus { get; set; }
        /// <summary>
        /// 退款总金额
        /// </summary>
        public decimal? RefundMoney { get; set; }

        /// <summary>
        /// 寄回时填的电话
        /// </summary>
        public string? SendBackMobile { get; set; }

        /// <summary>
        /// 退款详情id
        /// </summary>
        public Guid RefundId { get; set; }

        /// <summary>
        /// 退款详情Code
        /// </summary>
        public string? RefundCode { get; set; }



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
        /// <summary>快递单号.可null.</summary>
        public string? ExpressNu { get; set; }
        /// <summary>快递公司.可null.</summary>
        public string? ExpressCompanyName { get; set; }
        /// <summary>发物流时间.可null.</summary>
        public DateTime? SendExpressTime { get; set; }


        /// <summary>
        /// 收货地址 
        /// </summary>
        public string recvProvince { get; set; }
        /// <summary>
        /// 收货地址 
        /// </summary>
        public string recvCity { get; set; }
        /// <summary>
        /// 收货地址 
        /// </summary>
        public string recvArea { get; set; }
        /// <summary>
        /// 收货地址 
        /// </summary>
        public string recvPostalcode { get; set; }

        /// <summary>
        /// 是否是多物流
        /// </summary>
        public bool? IsMultipleExpress { get; set; }
    }



    public class ExpressData
    {

    }
}
#nullable disable
