using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 订单状态(留资)
    /// </summary>
    public enum OrderStatus
    {
        ///// <summary>
        ///// 待支付
        ///// </summary>
        //[Description("待支付")]
        //Unpaid = 1,

        ///// <summary>
        ///// 已支付
        ///// </summary>
        //[Description("已支付")]
        //Paid = 2,

        /// <summary>
        /// 待发货
        /// </summary>
        [Description("待发货")]
        UnDelivered = 3,

        /// <summary>
        /// 已发货
        /// </summary>
        [Description("已发货")]
        Delivered = 4,

        /// <summary>
        /// 退货中
        /// </summary>
        [Description("退货中")]
        Returning = 5,

        /// <summary>
        /// 已退货
        /// </summary>
        [Description("已退货")]
        Returned = 6,

        ///// <summary>
        ///// 已完成
        ///// </summary>
        //[Description("已完成")]
        //Completed = 7,

        /// <summary>
        /// 已取消
        /// </summary>
        [Description("已取消")]
        Cancelled = 8,

        ///// <summary>
        ///// 待评价
        ///// </summary>
        //[Description("待评价")]
        //UnEvaluated = 9,

        ///// <summary>
        ///// 已评价
        ///// </summary>
        //[Description("已评价")]
        //Evaluated = 10,


    }

    /// <summary>
    /// 订单状态v2
    /// </summary>
    public enum OrderStatusV2
    {
        ///// <summary>
        ///// 已创建.但是未进入支付流程        
        ///// </summary>
        //[Description("已创建")]
        //Created = 1,

        /// <summary>
        /// 整个购物流程已完成.要退货就是售后的事了
        /// </summary>
        [Description("已完成")]
        Completed = 333,

        /// <summary>
        /// 已取消|已关闭 
        /// 整个购物流程已取消退出
        /// </summary>
        [Description("已关闭")]
        Cancelled = 5,

        /// <summary>待支付|待付款</summary>
        [Description("待付款")]
        Unpaid = 101,
        /// <summary>支付中|付款中</summary>
        [Description("付款中")]
        Paiding = 102,
        /// <summary>已支付|已付款</summary>
        [Description("待发货")]
        Paid = 103,
        /// <summary>支付失败</summary>
        [Description("支付失败")]
        PaidFailed = 14,

        /// <summary>退款中</summary>
        [Description("退款中")]
        Refunding = 202,
        /// <summary>已退款</summary>
        [Description("已退款")]
        RefundOk = 203,
        /// <summary>退款失败</summary>
        [Description("退款失败")]
        RefundFailed = 204,




        /// <summary>待发货</summary>
        [Description("待发货")]
        Ship = 103,


        /// <summary>
        /// 已出库待收货（场景：已经向供应商下单，但暂时没有物流）
        /// </summary>
        [Description("待收货")]
        ExWarehouse = 301,
        /// <summary>发货中|出货中|待收货|商家已发货</summary>
        [Description("待收货")]
        Shipping = 302,

        /// <summary>
        /// 商品中有部分发货（订单中存在待发货和待收货状态，此时没有倒计时确认收货）
        /// </summary>
        [Description("部分发货")]
        PartialShipped = 303,
        /// <summary>已收货</summary>
        [Description("已收货")]
        Shipped = 333, 
    }

    ///// <summary>退货中</summary>
    //[Description("退货中")]
    //Returning = 402,
    ///// <summary>已退货</summary>
    //[Description("已退货")]
    //Returned = 403,
}
