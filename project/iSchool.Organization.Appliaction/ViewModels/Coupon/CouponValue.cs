using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Coupon
{
    public static class CouponValue
    {
        /// <summary>
        /// 获取券值
        /// </summary>
        /// <returns></returns>
        public static string GetCouponValue(CouponType couponType
            , decimal? fee
            , decimal? feeOver
            , decimal? priceOfTest
            , decimal? discount)
        {
            if (couponType == CouponType.LiJian) return $"{string.Format("{0:0.##}", fee.GetValueOrDefault())}元";
            if (couponType == CouponType.TiYan) return $"{string.Format("{0:0.##}", priceOfTest.GetValueOrDefault())}元";
            if (couponType == CouponType.ManJian) return $"{string.Format("{0:0.##}", feeOver.GetValueOrDefault())}-{string.Format("{0:0.##}", fee.GetValueOrDefault())}";
            if (couponType == CouponType.ZheKou) return $"{string.Format("{0:0.##}", discount * 10)}折";
            return "未知券类型，无法估值。";
        }

        public static string GetCouponExpireDate(CouponInfoVaildDateType couponInfoVaildDateType, DateTime startDate, DateTime endDate)
        {
            switch (couponInfoVaildDateType)
            {
                case CouponInfoVaildDateType.SpecialDate:
                case CouponInfoVaildDateType.SpecialDays:
                    return $"{startDate}至{endDate}";
                case CouponInfoVaildDateType.Forever:
                    return $"永久";
                default:
                    return $"非法类型";
            }
        }

    }
}
