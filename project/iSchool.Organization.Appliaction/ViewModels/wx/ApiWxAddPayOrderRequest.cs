using iSchool.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 调用微信下单(支付平台api)获取预支付info
    /// </summary>
    public class ApiWxAddPayOrderRequest
    {
        /// <summary>用户id</summary>
        public Guid UserId { get; set; }
        /// <summary>订单号</summary>
        public string OrderNo { get; set; }
        /// <summary>订单id</summary>
        public Guid OrderId { get; set; }
        /// <summary>订单类型(支付平台的类型)</summary>
        public int OrderType { get; set; } = 3;
        /// <summary>订单状态</summary>
        public int OrderStatus { get; set; }
        /// <summary>总金额</summary>
        public decimal TotalAmount { get; set; }
        /// <summary>支付金额(目前要与TotalAmount一样)</summary>
        public decimal PayAmount { get; set; }
        /// <summary>折扣金额</summary>
        public decimal DiscountAmount { get; set; } = 0;
        /// <summary>已退款金额</summary>
        public decimal RefundAmount { get; set; } = 0;
        /// <summary>备注</summary>
        public string Remark { get; set; }

        /// <inheritdoc cref="ApiOrderByProduct"/>
        public ApiOrderByProduct[] OrderByProducts { get; set; } = default!;
        /// <inheritdoc cref="ApiSubOrder"/>
        [Obsolete] public ApiSubOrder[] SubOrders { get; set; } = default!;

        /// <summary>微信openid</summary>
        public string OpenId { get; set; }
        /// <summary>
        /// 附加数据，在查询API和支付通知中原样返回，可作为自定义参数使用,长度string[1,128] <br/>
        /// eg: "from={平台}"
        /// </summary>
        public string Attach { get; set; }
        /// <summary>
        /// 订单来源系统
        /// </summary>
        public int System { get; set; } = 2;
        /// <summary>
        /// 是否需要支付 (可用于0元支付)
        /// </summary>
        public int NoNeedPay { get; set; }
        /// <summary>
        /// 是否重新支付
        /// </summary>
        public int IsRepay { get; set; } = 0;
        /// <summary>
        /// 小程序支付需要传1, 其他情况默认不管
        /// </summary>
        public int IsWechatMiniProgram { get; set; } = 0;
        /// <summary>
        /// appid主要用于小程序支付, 其他情况不管或传null
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 订单失效时间
        /// </summary>
        public DateTime? OrderExpireTime { get; set; }
        /// <summary>
        /// 运费总和
        /// </summary>
        public decimal FreightFee { get; set; } = 0;
    }

    /// <summary>订单产品信息</summary>
    public class ApiOrderByProduct
    {
        /// <summary>产品id</summary>
        public Guid ProductId { get; set; }
        /// <summary>
        /// 产品类型 1课程  2好物  3运费 <br/>
        /// 有运费时,$.Amount=$.Price=运费
        /// </summary>
        public int ProductType { get; set; }
        /// <summary>产品状态</summary>
        public int Status { get; set; }
        /// <summary>商品金额</summary>
        public decimal Amount { get; set; }
        /// <summary>产品备注</summary>
        public string Remark { get; set; }
        /// <summary>购买数量</summary>
        public int BuyNum { get; set; }
        /// <summary>单价</summary>
        public decimal Price { get; set; }
        /// <summary>预支付订单ID</summary>
        public Guid AdvanceOrderId { get; set; }
        /// <summary></summary>
        public Guid OrderDetailId { get; set; }
        /// <summary>子订单ID</summary>
        public Guid OrderId { get; set; }
    }

    /// <summary>子订单信息</summary>
    [Obsolete]
    public class ApiSubOrder
    {
        /// <summary>用户id</summary>
        public Guid UserId { get; set; }
        /// <summary>子订单id</summary>
        public Guid OrderId { get; set; }
        /// <summary>子订单no</summary>
        public string OrderNo { get; set; }
        /// <summary>子订单类型(支付平台的类型)</summary>
        public int OrderType { get; set; } = 3;
        /// <summary>子订单状态</summary>
        public int OrderStatus { get; set; }
        /// <summary>总金额</summary>
        public decimal TotalAmount { get; set; }
        /// <summary>支付金额(目前要与TotalAmount一样)</summary>
        public decimal PayAmount { get; set; }
        /// <summary>折扣金额</summary>
        public decimal DiscountAmount { get; set; } = 0;
        /// <summary>备注</summary>
        public string Remark { get; set; }
        /// <summary>订单来源系统</summary>
        public int System { get; set; } = 2;
        /// <summary>运费</summary>
        public decimal FreightFee { get; set; } = 0;
        /// <summary>订单流水号(就是AdvanceOrderNo)</summary>
        public string TradeNo { get; set; } = default!;
    }
}
