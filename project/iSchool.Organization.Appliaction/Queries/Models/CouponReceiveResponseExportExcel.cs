using NPOIHelper;
using System;
using System.Collections.Generic;
using iSchool.Infrastructure;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using System.Linq;
using System.Text;
using iSchool.Organization.Appliaction.ViewModels.Coupon;

namespace iSchool.Organization.Appliaction.Queries
{
    /// <summary>
    /// <see cref="CouponReceiveResponse"/>
    /// </summary>
    public class CouponReceiveResponseExportExcel
    {
        /// <summary>
        /// 领取id
        /// </summary>
        [ColumnType(Hide = true)]
        public Guid Id { get; set; }

        /// <summary>
        /// 领取编码
        /// </summary>
        [ColumnType(Name = "领取编码")]
        public string ReceiveNumber { get; set; }

        /// <summary>
        /// 领取人id
        /// </summary>
        [ColumnType(Hide = true)]
        public Guid UserId { get; set; }
        /// <summary>
        /// 领取人昵称
        /// </summary>
        [ColumnType(Name = "领取人昵称")]
        public string NickName { get; set; }

        /// <summary>
        /// 领取人手机号
        /// </summary>
        [ColumnType(Name = "领取人手机号")]
        public string Phone { get; set; }

        /// <summary>
        /// 券id
        /// </summary>
        [ColumnType(Hide = true)]
        public Guid CouponId { get; set; }
        /// <summary>
        /// 券名称
        /// </summary>
        [ColumnType(Name = "券名称")]
        public string CouponName { get; set; }
        /// <summary>
        /// 券编码
        /// </summary>
        [ColumnType(Hide = true)]
        public string CouponNumber { get; set; }
        /// <summary>
        /// 券值
        /// </summary>
        [ColumnType(Name = "券值")]
        public string CouponValue { get; set; }
        /// <summary>
        /// 券类型：1、体验券 2、折扣券 3、满减券  4.立减券
        /// </summary>
        [ColumnType(Hide = true)]
        public CouponType CouponType { get; set; }
        /// <summary>
        /// 优惠券类型
        /// </summary>
        [ColumnType(Name = "优惠券类型")]
        public string CouponTypeValue => CouponType.GetDesc();
        /// <summary>
        /// 发放范围
        /// </summary>
        [ColumnType(Hide = true)]
        public IEnumerable<EnableRangeSummary> EnableRangeSummaries { get; set; }
        /// <summary>
        /// 可用范围
        /// </summary>
        [ColumnType(Name = "可用范围")]
        public string EnableRangeString => CouponReceiveSummariesResponse.GetEnableRangeString(EnableRangeSummaries);
        /// <summary>
        /// 券使用规则
        /// </summary>
        [ColumnType(Name = "券使用规则")]
        public string CouponDesc { get; set; }
        /// <summary>
        /// 有效时间
        /// </summary>
        [ColumnType(Name = "有效时间")]
        public string ExpireDateString { get; set; }

        /// <summary>
        /// 领取状态
        /// </summary>
        [ColumnType(Hide = true)]
        public CouponReceiveStateExt Status { get; set; }

        /// <summary>
        /// 领取状态
        /// </summary>
        [ColumnType(Name = "领取状态")]
        public string CouponReceiveStateValue => Status.GetDesc();

        /// <summary>
        /// 下单订单Id
        /// 优惠券使用AdvanceOrderId
        /// </summary>
        [ColumnType(Hide = true)]
        public Guid? OrderId { get; set; }

        /// <summary>
        /// 下单订单号
        /// </summary>
        [ColumnType(Name = "下单订单号")]
        public string AdvanceOrderNo { get; set; }


        /// <summary>
        /// 优惠券使用AdvanceOrderId
        /// </summary>
        [ColumnType(Name = "下单时间")]
        public string OrderPaymentTime { get; set; }
        /// <summary>
        /// 下单商品及属性
        /// </summary>
        [ColumnType(Name = "下单商品及属性")]
        public string OrderDetailsString { get; set; }
        /// <summary>
        /// 商品原价
        /// </summary>
        [ColumnType(Name = "商品原价")]
        public string OrderOrgTotalAmountsString { get; set; }
        /// <summary>
        /// 优惠金额
        /// </summary>
        [ColumnType(Name = "优惠金额")]
        public string OrderDiscountsString { get; set; }
        /// <summary>
        /// 实付金额
        /// </summary>
        [ColumnType(Name = "实付金额")]
        public string OrderPaymentsString { get; set; }
    }
}