using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class OrderLogisticsDto
    {
        /// <summary>
        /// 订单id
        /// </summary>
        public Guid OrderId { get; set; }
        public Guid OrderDetailId { get; set; }
        public int Count { get; set; }
        /// <summary>
        /// 退款中数量
        /// </summary>
        public int RefundingCount { get; set; }
        /// <summary>
        /// 已退款数量
        /// </summary>
        public int RefundedCount { get; set; }

        public int LogisticCount { get; set; }
        /// <summary>
        /// 是否退款
        /// </summary>
        public int OrderStatus { get; set; }

        public KeyValuePair<string, string>[] Companys { get; set; }

        public List<LogisticData> LogisticDataList { get; set; }

    }
    public class LogisticData
    {
        public int Number { get; set; }
        /// <summary>物流订单号</summary>
        public string ExpressCode { get; set; }

        /// <summary>快递公司名称</summary>
        public string ExpressType { get; set; }
        /// <summary>
        ///发货时间
        /// </summary>
        public DateTime? SendExpressTime { get; set; }
    }

}


