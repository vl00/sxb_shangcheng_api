using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using NPOIHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.Queries
{
    public class CouponReceiveSummariesResponse
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
        [ColumnType(Hide = true)]
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

        [ColumnType(Hide = true)]
        public IEnumerable<EnableRangeSummary> EnableRangeSummaries { get; set; }


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


        public static string GetEnableRangeString(IEnumerable<EnableRangeSummary> range)
        {
            var sb = new StringBuilder();
            if (!(range?.Any() == true))
            {
                sb.Append("全平台商品");
                return sb.ToString();
            }

            foreach (var r in range)
            {
                switch (r.EnableRangeType)
                {
                    case CouponEnableRangeType.SpecialGoods:
                        var sku = r as SKURangeSummary;
                        sb.AppendLine($"{sku.FirstItemName} 总共:{sku.ItemTotals} 件商品。");
                        sb.AppendLine();
                        break;
                    case CouponEnableRangeType.SpecialGoodsType:
                        var goods = r as GoodTypeRangeSummary;
                        sb.AppendLine(goods.Name);
                        break;
                    case CouponEnableRangeType.SpcialBrand:
                        var brand = r as BrandRangeSummary;
                        sb.Append(brand.Name).Append(" ");
                        break;
                    case CouponEnableRangeType.Alls:
                        break;
                    default:
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 下单订单号
        /// 优惠券使用AdvanceOrderId
        /// </summary>
        [ColumnType(Name = "下单订单号")]
        public Guid? OrderId { get; set; }
    }

    public class CouponReceiveDetailResponse
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
        [ColumnType(Hide = true)]
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
        /// 券使用规则
        /// </summary>
        [ColumnType(Name = "券使用规则")]
        public string CouponDesc { get; set; }

        public IEnumerable<EnableRange> EnableRange { get; set; }
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


        public static string GetEnableRangeString(IEnumerable<EnableRange> range)
        {
            var sb = new StringBuilder();
            if (!(range?.Any() == true))
            {
                sb.Append("全平台商品");
                return sb.ToString();
            }

            foreach (var r in range)
            {
                switch (r.Type)
                {
                    case CouponEnableRangeType.SpecialGoods:
                        var sku = r as SKUEnableRange;
                        foreach (var item in sku.SKUItems)
                        {
                            sb.Append(item.CourseName);
                            int i = 0;
                            foreach (var prop in item.Properties)
                            {
                                sb.Append("+").Append("属性").Append(++i).Append("：").Append(prop.Name);
                            }
                            sb.Append("；");
                        }
                        break;
                    case CouponEnableRangeType.SpecialGoodsType:
                        var goods = r as GoodsTypeEnableRange;
                        sb.Append(goods.Name).Append(" ");
                        break;
                    case CouponEnableRangeType.SpcialBrand:
                        var brand = r as CourseBrandEnableRange;
                        sb.Append(brand.Name).Append(" ");
                        break;
                    case CouponEnableRangeType.Alls:
                        break;
                    default:
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 下单订单号
        /// 优惠券使用AdvanceOrderId
        /// </summary>
        [ColumnType(Name = "下单订单号")]
        public Guid? OrderId { get; set; }
    }
}