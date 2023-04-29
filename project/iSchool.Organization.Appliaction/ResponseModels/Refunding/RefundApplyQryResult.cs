using iSchool.Infrastructure;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class RefundApplyQryResult
    {
        /// <summary>订单详情id</summary>
        public Guid OrderDetailId { get; set; }

        public CourseOrderProdItemDto Item { get; set; } = default!;

        /// <summary>订单支付时间</summary>
        [JsonConverter(typeof(DateTimeToTimestampJsonConverter))]
        public DateTime Paytime => _Order?.Paymenttime! ?? default;

        /// <summary>是否极速退款</summary>
        public bool IsFastRefund => RefundType == (int)RefundTypeEnum.FastRefund;
        /// <summary>退款类型</summary>
        [JsonIgnore]
        public int RefundType { get; set; }

        /// <summary>(当前可以)退款数量</summary>
        public int RefundCount => Item?.BuyCount ?? 0;
        /// <summary>
        /// 退款金额<br/>
        /// 不需要判断是否最后一单而加上运费 
        /// </summary>
        public decimal RefundMoney { get; set; } // >=Item?.PricesAll ?? 0

        /// <summary>
        /// 用于不是极速退款时,给用户选择的退款服务类型s
        /// </summary>
        public IEnumerable<KeyValuePair<int, string>>? RefundServiceTypes { get; set; } = default!;
        /// <summary>仅退款 的 退款理由选择项s</summary>
        public IEnumerable<KeyValuePair<int, string>>? RefundCauses1 { get; set; } = default!;
        /// <summary>退货退款 的 退款理由选择项s</summary>
        public IEnumerable<KeyValuePair<int, string>>? RefundCauses2 { get; set; } = default!;
        /// <summary>退货退款 的 选择退货方式s</summary>
        public IEnumerable<KeyValuePair<int, string>>? ReturnModes2 { get; set; } = default!;

        public Guid OrderId => _OrderDetial.Orderid;
        [JsonIgnore] public Order _Order { get; set; } = default!;
        [JsonIgnore] public OrderDetial _OrderDetial { get; set; } = default!;
        [JsonIgnore] public (int OkCount, int RefundingCount) _RfdCounts { get; set; }
    }

#nullable disable
}
