using iSchool.Infrastructure;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class WxPayOkOrderDto
    {
        /// <summary>是否支付成功</summary>
        public bool? PayIsOk { get; set; }

        /// <summary>
        /// false = 一般订单 根据orderNo或orderId跳详情页 <br/>
        /// true = 该订单被拆分成多个子订单 跳待收货列表
        /// </summary>
        public bool IsAdvance => AdvanceOrderId != null;

        /// <summary>用户支付时间</summary>
        public DateTime? UserPayTime { get; set; }
        /// <summary>订单金额</summary>
        public decimal? Paymoney { get; set; }
        /// <summary>
        /// 订单类型 <br/>
        /// <see cref="Domain.Enum.OrderType"/>
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>订单id. 多子订单时为预订单id</summary>        
        public Guid OrderId { get; set; }
        /// <summary>订单号. 多子订单时为预订单号</summary>
        public string OrderNo { get; set; } = default!;

        public Guid UserId { get; set; }

        #region 单个商品
        //public int BuyCount => BuyAmount;
        //public int BuyAmount { get; set; }
        //public Guid CourseId { get; set; }
        //public Guid GoodsId { get; set; }
        #endregion 单个商品

        /// <summary>预订单id.(单个订单是为null)</summary>
        public Guid? AdvanceOrderId { get; set; }
        /// <summary>预订单号（不为null,可以当成多子订单的组no）</summary>
        public string AdvanceOrderNo { get; set; } = default!;

        /// <summary>
        /// //多商品 （单商品时为null）<br/>
        /// 商品
        /// [(orderDetailId, orderId, goodsId, courseId, buyCount),...]
        /// </summary>
        public (Guid OrderDetailId, Guid OrderId, Guid GoodsId, Guid CourseId, int BuyCount)[]? Prods { get; set; }

        /// <summary>上级顾问</summary>
        public string? FxHeaducode { get; set; }
        /// <summary>版本</summary>
        public string? _Ver { get; set; }

        public Guid? _Modifier { get; set; }
    }

#nullable disable
}
