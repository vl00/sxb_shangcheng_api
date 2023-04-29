using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class RwInviteActivityUserOrderStoreStatusLsItem
    {
        /// <summary>订单id</summary>
        public Guid OrderId { get; set; }
        /// <summary>订单号</summary>
        public string OrderNo { get; set; } = default!;
        /// <summary>OrderDetail id</summary>
        public Guid OrderDetailId { get; set; }
        /// <summary>数量</summary>
        public int Count { get; set; }
        /// <summary>订单创建时间</summary>
        public DateTime CreateTime { get; set; }
        /// <summary>订单状态</summary>
        public int Status { get; set; }
        /// <summary>订单状态（中文）</summary>
        public string? StatusDesc { get; set; }
        /// <summary>消费的rw积分</summary>
        public double ConsumedScores { get; set; }
    }

#nullable disable
}
