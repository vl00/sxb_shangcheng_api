using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class OrderProdItemDto
    {
        public Guid OrderDetailId { get; set; }
        public string? _Ver { get; set; }

        /// <summary>
        /// 内容类型<br/>        
        /// 1=课程 2=好物
        /// </summary>        
        public int ProdType { get; set; }

        /// <summary>单价</summary>
        public decimal Price { get; set; }
        /// <summary>原价.可null</summary>
        public decimal? Origprice { get; set; } = null;

        /// <summary>单价*数量</summary>
        public decimal PricesAll => Price * BuyCount;
        /// <summary>
        /// 一个orderdetail总金额 (优惠后的PricesAll可能不等于这个)
        /// </summary>
        public decimal Payment { get; set; }

        /// <summary>
        /// Order Detail优惠金额
        /// </summary>
        public decimal CouponAmount { get; set; }


        /// <summary>数量</summary>        
        public int BuyCount { get; set; } = 1;

        /// <summary>
        /// 数量.<br/>
        /// 发现起错名字了,改用buyCount吧
        /// </summary>
        [Obsolete("发现起错名字了,改用buyCount吧")]
        public int BuyAmount
        {
            get => BuyCount;
            set => BuyCount = value;
        }

        /// <summary>
        /// 已退款数
        /// </summary>
        public int RefundedCount { get; set; } = 0;

        /// <summary>
        /// 退款中数量
        /// </summary>
        public int RefundingCount { get; set; } = 0;

        /// <summary>
        /// 申请退货数量
        /// </summary>
        public int RefundCount { get; set; }

        /// <summary>退款金额</summary>
        public decimal? RefundMoney { get; set; }

        /// <summary>
        /// 商品是否有效.`true=有效, false=无效`<br/>
        /// 注：商品无效不一定是下架, 商品可以被后台设置为不显示
        /// </summary>   
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// 商品ID
        /// </summary>
        [JsonIgnore]
        public Guid ProductId { get; set; }

        /// <summary>
        /// 商品Title
        /// </summary>
        [JsonIgnore]
        public string? ProductTitle { get; set; }


        /// <summary>
        /// 商品积分信息
        /// </summary>
        public PointsExchangeInfo? PointsInfo { get; set; }
    }

#if DEBUG
    /// <summary>for OrderProdItemDto的具体类型字段s</summary>
    public class Apidoc_OrderProdItemDto
    {
        public CourseOrderProdItemDto? __apidoc_ProdType_1 { get; set; } = null;
    }
#endif

    /// <summary>
    /// 课程order内容item
    /// </summary>
    public class CourseOrderProdItemDto : OrderProdItemDto
    {
        /// <summary>课程id</summary>
        public Guid Id { get; set; }
        /// <summary>课程短id</summary>
        public string Id_s { get; set; } = default!;
        /// <summary>课程名称</summary>
        public string Title { get; set; } = default!;
        /// <summary>课程副标题</summary>
        public string? Subtitle { get; set; }

        ///// <summary>科目(中文)</summary>
        //public string SubjectDesc { get; set; } = default!;
        ///// <summary>科目</summary>
        //public int Subject { get; set; }

        /// <summary>课程banner图片地址</summary>
        public string[] Banner { get; set; } = default!;

        #region 关联机构信息
        /// <summary>机构是否认证（true：认证；false：未认证）</summary>
        public bool Authentication => OrgInfo?.Authentication ?? false;
        #endregion
        /// <summary>机构信息</summary>
        public CourseOrderProdItem_OrgItemDto OrgInfo { get; set; } = default!;


        /// <summary>库存</summary>
        public int Stock { get; set; }
        /// <summary>商品id</summary>
        public Guid GoodsId { get; set; }


        /// <summary>属性项名称s</summary>
        public string[] PropItemNames { get; set; } = default!;
        /// <summary>属性项IDs</summary>
        public Guid[] PropItemIds { get; set; } = default!;

        /// <summary>新人专享</summary> 
        public bool? NewUserExclusive { get; set; }
        /// <summary>限时优惠</summary> 
        public bool? LimitedTimeOffer { get; set; }

        /// <summary>订单状态(中文)</summary>
        public string StatusDesc { get; set; } = default!;
        /// <summary>订单状态</summary>
        public int Status { get; set; }

        [JsonIgnore]
        public JObject? _ctn { get; set; }

        /// <summary>供应商信息</summary>
        public CourseOrderProdItem_SupplierInfo? SupplierInfo { get; set; }
    }

    public class CourseOrderProdItem_OrgItemDto
    {
        /// <summary>机构id</summary>
        public Guid Id { get; set; }
        /// <summary>机构短id</summary>
        public string Id_s { get; set; } = default!;
        /// <summary>机构名</summary>
        public string Name { get; set; } = default!;
        /// <summary>机构logo</summary>
        public string? Logo { get; set; }
        /// <summary>是否已认证</summary>
        public bool Authentication { get; set; }
        /// <summary>描述</summary> 
        public string? Desc { get; set; }
        /// <summary>子描述</summary> 
        public string? Subdesc { get; set; }
    }

    public class CourseOrderProdItem_SupplierInfo
    {
        /// <summary>供应商id.可null</summary> 
        public Guid? Id { get; set; }
        /// <summary>供应商名.可null(一般前端不显示不返回)</summary> 
        public string? Name { get; set; }
    }

    /// <summary>
    /// OrderDetial表ctn字段
    /// </summary>
    public class CourseGoodsOrderCtnDto
    {
        /// <summary>课程id</summary>
        public Guid Id { get; set; }
        /// <summary>课程no</summary>
        public long No { get; set; }
        /// <summary>课程名称</summary>
        public string Title { get; set; } = default!;
        /// <summary>课程副标题</summary>
        public string? Subtitle { get; set; }

        /// <summary>类型 1=课程 2=好物</summary>
        public int ProdType { get; set; }

        ///// <summary>科目</summary>
        //public int? Subject { get; set; }

        /// <summary>课程banner图片地址</summary>
        public string Banner { get; set; } = default!;

        #region 关联机构信息
        /// <summary>机构是否认证（true：认证；false：未认证）</summary>
        public bool Authentication { get; set; }
        /// <summary>机构id</summary>
        public Guid OrgId { get; set; }
        /// <summary>机构短id</summary>
        public long OrgNo { get; set; }
        /// <summary>机构名</summary>
        public string OrgName { get; set; } = default!;
        /// <summary>机构logo</summary>
        public string? OrgLogo { get; set; }
        /// <summary>描述</summary> 
        public string? OrgDesc { get; set; }
        /// <summary>子描述</summary> 
        public string? OrgSubdesc { get; set; }
        #endregion

        /// <summary>商品id</summary>
        public Guid GoodsId { get; set; }
        /// <summary>属性项名称s</summary>
        public string[] PropItemNames { get; set; } = default!;
        /// <summary>属性项IDs</summary>
        public Guid[] PropItemIds { get; set; } = default!;

        /// <summary>是否新人专享</summary>
        public bool IsNewUserExclusive { get; set; }

        /// <summary>供应商</summary> 
		public Guid? SupplierId { get; set; }
        /// <summary>成本价</summary> 
		public decimal? Costprice { get; set; }
        /// <summary>货号</summary> 
        public string? ArticleNo { get; set; }

        public string? _Ver { get; set; }
        public string? _FxHeaducode { get; set; }
        public bool? _prebindFxHead_ok { get; set; }

        public RwInviteActivity? _RwInviteActivity { get; set; }
        /// <summary>rw活动</summary> 
        public class RwInviteActivity
        {
            public string UnionID { get; set; } = default!;
            public double ConsumedScores { get; set; }
            public CourseExchange CourseExchange { get; set; } = default!;
        }

        public FreezeMoneyInLogIdDto? _FreezeMoneyInLogIds { get; set; }
        /// <summary>冻结奖励</summary> 
        public class FreezeMoneyInLogIdDto
        {
            public string Id { get; set; } = default!;
            /// <summary>0 = SignIn(签到), 1 = OrgGoodNewReward(机构新人返现)</summary>
            public int Type { get; set; }
        }
    }
}
#nullable disable
