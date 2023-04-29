using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// use `Consts.Err.XXX` instead
    /// </summary>
    [Obsolete("use `Consts.Err.XXX` instead")]    
    public static class OrderCreateError
    {
        /// <summary>已支付</summary>
        public const int PaidBefore = 1;
        /// <summary>课程已下架</summary>
        public const int CourseOffline = 2;
        /// <summary>价格已改变</summary>
        public const int PriceChanged = 3;
        /// <summary>没库存了</summary>
        public const int NoStock = 4;
        /// <summary>商品已下架</summary>
        public const int CourseGoodsOffline = 5;

        /// <summary>创建订单失败</summary>
        public const int OrderCreateFailed = 444101;
        /// <summary>call预支付接口失败</summary>
        public const int CallPaidApiError = 444102;
        /// <summary>创建轮询缓存</summary>
        public const int PollError = 444103;
        /// <summary>call检查订单状态接口失败</summary>
        public const int CallCheckPaystatusError = 444104;
        /// <summary>更新之前的订单失败</summary>
        public const int PrevOrderUpdateFailed = 444105;
    }
}
