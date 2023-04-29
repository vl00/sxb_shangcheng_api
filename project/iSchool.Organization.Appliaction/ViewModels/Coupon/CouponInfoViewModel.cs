using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Coupon
{
    public class CouponInfos
    {

        public int Total { get; set; }

        public List<CouponInfoSummaryItem> Items { get; set; }
    }

    public class BrandCouDanCoupon
    {
        public Guid BrandId { get; set; }
        public List<CouDanCoupon> CouDanCoupons { get; set; }
    }

    public class CouDanCoupon
    {
        public bool IsBind { get; set; }
        public CouponInfo CouponInfo { get; set; }


    }

    public class CouponInfo
    {

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public CouponInfoVaildDateType VaildDateType { get; set; }
        public DateTime? VaildStartDate { get; set; }
        public DateTime? VaildEndDate { get; set; }

        /// <summary>
        /// Unit = Hours
        /// </summary>
        public double VaildTime { get; set; }

        public int MaxTake { get; set; }
        public int Stock { get; set; }
        public CouponType CouponType { get; set; }
        public decimal? Fee { get; set; }
        public decimal? FeeOver { get; set; }
        public decimal? Discount { get; set; }
        public decimal? MaxFee { get; set; }
        /// <summary>
        /// 体验价格
        /// </summary>
        public decimal? PriceOfTest { get; set; }
        public string Link { get; set; }

        /// <summary>
        /// 1 上线 0 下线
        /// </summary>
        public int Status { get; set; }
        public string ICon { get; set; }

        public string Number { get; set; }

        [JsonIgnore]
        public string EnableRange_JSN { get; set; }

        /// <summary>
        /// 是否为绑定使用券(这张券针对某些SKUID是绑定使用，并非这张券本身绑定使用)
        /// </summary>
        public bool? IsBindUse { get; set; }



    }

    public class CouponInfoSummaryItem
    {
        /// <summary>
        /// 券ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 券类型
        /// </summary>
        public string Number { get; set; }

        public CouponType Type { get; set; }

        /// <summary>
        /// 券标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 券值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 用户限领
        /// </summary>
        public int MaxTake { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public int Total { get; set; }


        /// <summary>
        /// 有效日期
        /// </summary>
        public string ValidTime { get; set; }



        /// <summary>
        /// 使用规则说明
        /// </summary>
        public string RuleDesc { get; set; }

        public CouponState State { get; set; }




        public decimal? Fee { get; set; }
        public decimal? FeeOver { get; set; }
        public decimal? Discount { get; set; }
        /// <summary>
        /// 体验价格
        /// </summary>
        public decimal? PriceOfTest { get; set; }
        public CouponInfoVaildDateType VaildDateType { get; set; }
        public DateTime? VaildStartDate { get; set; }
        public DateTime? VaildEndDate { get; set; }

        /// <summary>
        /// Unit = Day
        /// </summary>
        public int VaildDay { get; set; }

        public IEnumerable<EnableRangeSummary> EnableRangeSummaries { get; set; }



    }

    public class CouponInfoItem
    {
        /// <summary>
        /// 券ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 券类型
        /// </summary>
        public string Number { get; set; }

        public CouponType Type { get; set; }

        /// <summary>
        /// 券标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 券值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 用户限领
        /// </summary>
        public int MaxTake { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public int Total { get; set; }


        /// <summary>
        /// 有效日期
        /// </summary>
        public string ValidTime { get; set; }


        /// <summary>
        /// 可用范围
        /// </summary>
        public IEnumerable<dynamic> EnableRange { get; set; }

        /// <summary>
        /// 使用规则说明
        /// </summary>
        public string RuleDesc { get; set; }

        public CouponState State { get; set; }




        public decimal? Fee { get; set; }
        public decimal? FeeOver { get; set; }
        public decimal? Discount { get; set; }
        /// <summary>
        /// 体验价格
        /// </summary>
        public decimal? PriceOfTest { get; set; }
        public CouponInfoVaildDateType VaildDateType { get; set; }
        public DateTime? VaildStartDate { get; set; }
        public DateTime? VaildEndDate { get; set; }

        /// <summary>
        /// Unit = Day
        /// </summary>
        public int VaildDay { get; set; }








    }

    public class CouponInfoReceiveState
    {


        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }

        public CouponInfoVaildDateType VaildDateType { get; set; }

        public DateTime? VaildStartDate { get; set; }
        public DateTime? VaildEndDate { get; set; }

        /// <summary>
        /// Unit = Hours
        /// </summary>
        public double VaildTime { get; set; }

        public int MaxTake { get; set; }
        public int Stock { get; set; }
        public CouponType CouponType { get; set; }
        public decimal? Fee { get; set; }
        public decimal? FeeOver { get; set; }
        public decimal? Discount { get; set; }
        /// <summary>
        /// 体验价格
        /// </summary>
        public decimal? PriceOfTest { get; set; }

        public string Number { get; set; }

        public bool ReceiveState { get; set; }

        /// <summary>
        /// 预估金额
        /// </summary>
        public decimal? EstimatedAmount { get; set; }


        /// <summary>
        /// 是否为品牌券
        /// </summary>
        public bool IsBrandCoupon { get; set; }

    }


    public class CouponReceive
    {
        public Guid Id { get; set; }
        public string Number { get; set; }

        public Guid CouponId { get; set; }

        /// <summary>
        /// 优惠券使用AdvanceOrderId
        /// </summary>
        public Guid? OrderId { get; set; }

        public Guid UserId { get; set; }

        public DateTime GetTime { get; set; }

        public CouponInfoVaildDateType VaildDateType { get; set; }

        /// <summary>
        /// 有效开始时间
        /// </summary>
        public DateTime VaildStartTime { get; set; }
        /// <summary>
        /// 有效结束时间
        /// </summary>
        public DateTime VaildEndTime { get; set; }

        public DateTime? UsedTime { get; set; }

        public CouponReceiveState Status { get; set; }

        public string Remark { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public CouponType CouponType { get; set; }
        public decimal? Fee { get; set; }
        public decimal? FeeOver { get; set; }
        public decimal? Discount { get; set; }
        /// <summary>
        /// 体验价格
        /// </summary>
        public decimal? PriceOfTest { get; set; }

        public string CouponNumber { get; set; }



        public CouponReceiveFlagState FlagState
        {
            get
            {
                CouponReceiveFlagState state = CouponReceiveFlagState.Default;
                var now = DateTime.Now;
                if ((now - GetTime).TotalHours <= 48)
                {
                    state = CouponReceiveFlagState.Newer;
                }
                if (VaildEndTime > now && (VaildEndTime - now).TotalDays <= 3)
                {
                    state = CouponReceiveFlagState.WillExpire;
                }
                return state;
            }
        }


        public CouponReceiveStateExt StatusExt
        {
            get
            {
                var now = DateTime.Now;
                if (Status == CouponReceiveState.WaitUse && VaildEndTime < now)
                {
                    return CouponReceiveStateExt.Expire;
                }
                return (CouponReceiveStateExt)Status;
            }
        }

        /// <summary>
        /// 是否为品牌券
        /// </summary>
        public bool IsBrandCoupon { get; set; }

        /// <summary>
        /// 预估金额
        /// </summary>
        public decimal? EstimatedAmount { get; set; }

        [JsonIgnore]
        public string EnableRange_JSN { get; set; }

    }

    /// <summary>
    /// 标识状态
    /// </summary>
    public enum CouponReceiveFlagState
    {
        Default = 0,
        /// <summary>
        /// 新到
        /// </summary>
        Newer = 1,
        /// <summary>
        /// 即将过期
        /// </summary>
        WillExpire = 2,



    }
    /// <summary>
    /// 优惠券领取大纲，（承载某各用户对某张优惠券的领取情况）
    /// </summary>
    public class CouponReceiveSummary
    {

        public CouponReceiveSummary(Guid userId, Guid couponId)
        {
            this.UserId = userId;
            this.CouponId = couponId;

        }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 券ID
        /// </summary>
        public Guid CouponId { get; set; }


        /// <summary>
        /// 总共领取张数
        /// </summary>
        public int TotalReceive { get; set; }
        /// <summary>
        /// 未使用数量
        /// </summary>
        public int UnUseCount { get; set; }


        /// <summary>
        /// 过期但未使用
        /// </summary>
        public int ExipreUnUseCount { get; set; }


    }

    public class OrderUseCouponInfo
    {
        public Guid OrderId { get; set; }
        public string CouponName { get; set; }
        public decimal? CouponAmount { get; set; }

        public decimal? TotalPayment { get; set; }

    }

    public enum CouponState
    {
        /// <summary>
        /// 已上架
        /// </summary>
        Online = 1,
        /// <summary>
        /// 已下架
        /// </summary>
        Offline = 2,
        /// <summary>
        /// 隐形上架
        /// </summary>
        HideOnline = 3,
        /// <summary>
        /// 已失效
        /// </summary>
        LoseEfficacy = 4,

    }


    public class CouponStateValue {



        public CouponState CouponState { get; set; }

        public CouponStateValue(CouponInfoState status, bool isHide)
        {
            if (status == CouponInfoState.Offline) {
                CouponState = CouponState.Offline;
                return;

            }
            if (status == CouponInfoState.Online && isHide) {
                CouponState = CouponState.HideOnline;
                return;
            };
            if (status == CouponInfoState.LoseEfficacy) {
                CouponState = CouponState.LoseEfficacy;
                return;
            }
            else {
                CouponState = CouponState.Online;
                return;
            }

        }

    }

    public class CouponVaildTimeFormatter
    {
        CouponInfoVaildDateType vaildDateType;
        DateTime? vaildStartDate;
        DateTime? vaildEndDate;
        long? vaildTime;

        public CouponVaildTimeFormatter(CouponInfoVaildDateType vaildDateType, DateTime? vaildStartDate, DateTime? vaildEndDate, long? vaildTime)
        {
            this.vaildDateType = vaildDateType;
            this.vaildStartDate = vaildStartDate;
            this.vaildEndDate = vaildEndDate;
            this.vaildTime = vaildTime;


        }

        /// <summary>
        /// 后台列表的格式化
        /// </summary>
        /// <returns></returns>
        public string BGListFormatt()
        {
            if (vaildDateType == CouponInfoVaildDateType.Forever) return "永久有效";
            if (vaildDateType == CouponInfoVaildDateType.SpecialDate) return $"{vaildStartDate.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm")} - {vaildEndDate.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm")}";
            if (vaildDateType == CouponInfoVaildDateType.SpecialDays) return $"领券后{TimeSpan.FromHours(vaildTime.GetValueOrDefault()).TotalDays}天有效";
            return "未知过期类型，无法评估过期时间。";
        }
    }



    /// <summary>
    /// 可用范围概览(范围内容太广了，概览折叠显示)
    /// </summary>
    public class EnableRangeSummary {
        public CouponEnableRangeType EnableRangeType  { get; set; }
    }

    /// <summary>
    /// 指定SKU范围概览
    /// </summary>
    public class SKURangeSummary: EnableRangeSummary
    {
        public Guid FirstItemId { get; set; }

        public string FirstItemName { get; set; }

        public long ItemTotals { get; set; }

    }

    /// <summary>
    /// 指定商品类型概览
    /// </summary>
    public class GoodTypeRangeSummary : EnableRangeSummary
    {
        public int Id { get; set; }

        public string   Name { get; set; }
    }

    /// <summary>
    /// 指定品牌范围概览
    /// </summary>
    public class BrandRangeSummary : EnableRangeSummary
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    /// 优惠专区
    /// </summary>
    public class DiscountAreaPglist
    {
        public int Index { get; set; }
        /// <summary>
        /// 券的类型
        /// </summary>
        public object CouponList { get; set; }
        /// <summary>
        /// 商品的列表
        /// </summary>
        public ResponseModels.CoursesByOrgIdQueryResponse CourseList { get; set; }
    }




}
