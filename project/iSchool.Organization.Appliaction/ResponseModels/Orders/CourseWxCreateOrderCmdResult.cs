using iSchool.Organization.Appliaction.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    [Obsolete("1.9-")]
    public class CourseWxCreateOrderCmdResult
    {
        /// <summary>
        /// 表示是否成功<br/>
        /// 0=成功<br/>
        /// 1=系统繁忙<br/>
        /// 2=数据被修改,需要重新刷新页面<br/>
        /// </summary>
        public int Errcode { get; set; }
        /// <summary>错误消息.</summary>
        public string? Errmsg { get; set; }

        /// <summary>订单号.正常时不会为null.</summary>
        public Guid? OrderId { get; set; }
        /// <summary>用于轮询.正常时不会为null.</summary>
        public string? PollId { get; set; }

        /// <summary>用于wx预订单号</summary>        
        public JToken? WxPayResult { get; set; }

        /// <summary>
        /// 此值不为null时表示返回无效的商品ids
        /// </summary>
        public List<Guid>? NotValidSkus { get; set; }
    }

    public class CourseWxCreateOrderCmdResult_v4
    {
        /// <summary>用于wx预订单号</summary>       
        public JToken? WxPayResult { get; set; }
        /// <summary>用于轮询.正常时不会为null.</summary>
        public string? PollId { get; set; }

        /// <summary>预订单id</summary>       
        public Guid? OrderId { get; set; }
        /// <summary>预订单号</summary>      
        public string? OrderNo { get; set; }

        /// <summary>无效(下架)的商品s</summary>
        public List<CourseGoodsSimpleInfoDto>? NotValids { get; set; } = new List<CourseGoodsSimpleInfoDto>();
        /// <summary>无库存的商品s</summary>
        public List<CourseGoodsSimpleInfoDto>? NoStocks { get; set; } = new List<CourseGoodsSimpleInfoDto>();
        /// <summary>价格变动的商品s</summary>
        public List<CourseGoodsSimpleInfoDto>? PriceChangeds { get; set; } = new List<CourseGoodsSimpleInfoDto>();
        /// <summary>rw活动积分不够的商品s</summary>
        public List<CourseGoodsSimpleInfoDto>? NoRwScores { get; set; } = new List<CourseGoodsSimpleInfoDto>();
        /// <summary>不发货地区的skuids</summary>
        public List<Guid>? BlacklistSkuIds { get; set; }

        /// <summary>预订单id</summary>       
        public Guid? AdvanceOrderId => OrderId;
        /// <summary>预订单号</summary>      
        public string? AdvanceOrderNo => OrderNo;
        /// <summary>实际总支付金额</summary>      
        public decimal? Totalpayment { get; set; }
    }

    //public class PullWxPreAddPayOrderCmdResult
    //{
    //    /// <summary>用于wx预订单号</summary>       
    //    public JToken? WxPayResult { get; set; }
    //    /// <summary>用于轮询.正常时不会为null.</summary>
    //    public string? PollId { get; set; }

    //    /// <summary>预订单id</summary>       
    //    public Guid? OrderId { get; set; }
    //    /// <summary>预订单号</summary>      
    //    public string? OrderNo { get; set; }
    //    /// <summary>预订单id</summary>       
    //    public Guid? AdvanceOrderId => OrderId;
    //    /// <summary>预订单号</summary>      
    //    public string? AdvanceOrderNo => OrderNo;

    //    /// <summary>实际总支付金额</summary>      
    //    public decimal? Totalpayment { get; set; }
    //}

#nullable disable
}
