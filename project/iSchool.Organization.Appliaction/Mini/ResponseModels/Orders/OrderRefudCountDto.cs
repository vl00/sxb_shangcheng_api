using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class OrderRefudCountDto
    {
        public Guid OrderId { get; set; }

        public Guid? OrderDetailId { get; set; }

        public Guid? GoodId { get; set; }
        /// <summary>
        /// 已退款数量
        /// </summary>
        public int RefundedCount { get; set; }


        /// <summary>
        /// 退款中数量
        /// </summary>
        public int RefundingCount { get; set; }
    }
}
