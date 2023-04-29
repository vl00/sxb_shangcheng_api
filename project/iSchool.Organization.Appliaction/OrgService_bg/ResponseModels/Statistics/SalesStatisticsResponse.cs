using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.Statistics
{
    [Serializable]
    public class SalesStatisticsResponse
    {
        /// <summary>
        /// 今日订单数
        /// </summary>
        public int TodayOrderCount { get; set; }

        /// <summary>
        /// 今日销售额
        /// </summary>
        public decimal TodaySales { get; set; }


        /// <summary>
        /// 今日支付人数
        /// </summary>
        public int TodayPayCount { get; set; }


        /// <summary>
        /// 今日支付订单数
        /// </summary>
        public int TodayOrderPayCount { get; set; }


        /// <summary>
        /// 今日复购人数
        /// </summary>
        public int TodayRepurchase { get; set; }

        /// <summary>
        /// 销售额
        /// </summary>
        public decimal AllSale { get; set; }

        public List<View> SalesView { get; set; }
    }



    public class View
    {
        public string Day { get; set; }

        /// <summary>
        /// 订单数
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// 销售额
        /// </summary>
        public decimal Sales { get; set; }

        /// <summary>
        /// 支付人数
        /// </summary>
        public int PayUserCount { get; set; }

        /// <summary>
        /// 支付笔数
        /// </summary>
        public int PayCount { get; set; }

        /// <summary>
        /// 复购数
        /// </summary>
        public int RepurChase { get; set; }
    }
}
