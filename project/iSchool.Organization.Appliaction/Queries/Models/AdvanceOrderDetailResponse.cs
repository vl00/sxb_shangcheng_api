using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Queries.Models
{
    public partial class AdvanceOrderDetailResponse
    {
        /// <summary>
        /// 支付订单号
        /// </summary>
        public string AdvanceOrderNo { get; set; }
        /// <summary>
        /// 支付时间/下单时间
        /// </summary>
        public string PaymentTime { get; set; }

        /// <summary>
        /// 商品详情
        /// </summary>
        public IEnumerable<AdvanceOrderDetail> OrderDetails { get; set; }
    }
}