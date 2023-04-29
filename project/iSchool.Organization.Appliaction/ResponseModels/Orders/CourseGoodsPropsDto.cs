using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// 课程套餐属性结果
    /// </summary>
    public class CourseGoodsPropsDto
    {
        /// <summary>课程id</summary>
        public Guid Id { get; set; }
        /// <summary>课程title</summary>
        public string Title { get; set; } = default!;
        /// <summary>课程图标</summary>
        public string? Logo { get; set; }
        /// <summary>最小价格</summary>
        public decimal MinPrice { get; set; }
        /// <summary>最大价格</summary>
        public decimal MaxPrice { get; set; }

        /// <summary>
        /// 最小积分
        /// </summary>
        public int? MinPoints { get; set; }

        /// <summary>
        /// 最大积分
        /// </summary>
        public int? MaxPoints { get; set; }

        /// <summary>购买数量</summary>
        public int BuyAmount { get; set; } = 1;

        /// <summary>课程属性s列表.已排序</summary>
        public IEnumerable<CoursePropsListItemDto> Props { get; set; } = default!;

        /// <summary>
        /// 商品与(套餐)属性项的关系表.<br/>
        /// 用于多分组时选中某项并屏蔽其他项.
        /// </summary>
        public IEnumerable<CourseGoodsPropsSmTableItemDto> Table { get; set; } = default!;
    }

    /// <summary>
    /// 课程属性s列表项dto
    /// </summary>
    public class CoursePropsListItemDto
    {
        /// <summary>属性id</summary>
        public Guid Id { get; set; }
        /// <summary>属性名</summary>
        public string Name { get; set; } = default!;
        public string Cover { get; set; } = default!;
        /// <summary>课程属性s的属性项s列表.已排序</summary>
        public IEnumerable<CoursePropItemsListItemDto> PropItems { get; set; } = default!;

        public int Sort_pg { get; set; }
    }

    /// <summary>
    /// 课程属性s的属性项s列表项dto
    /// </summary>
    public class CoursePropItemsListItemDto
    {
        /// <summary>属性项id</summary>
        public Guid Id { get; set; }
        /// <summary>属性项名</summary>
        public string Name { get; set; } = default!;
        public string Cover { get; set; } = default!;
        public int Sort_i { get; set; }
    }

    /// <summary>
    /// 课程商品信息
    /// </summary>
    public class CourseGoodsSimpleInfoDto
    {
        /// <summary>商品id</summary>
        public Guid Id { get; set; }
        /// <summary>课程id</summary>
        public Guid CourseId { get; set; }
        /// <summary>库存</summary>
        public int Stock { get; set; }
        /// <summary>现在价格</summary>
        public decimal Price { get; set; }
        /// <summary>原始价格.可为null.</summary>
        public decimal? Origprice { get; set; }
        /// <summary>属性项s</summary>
        public CoursePropItemsListItemDto[] PropItems { get; set; } = default!;

        /// <summary>本次限购数量(spu和sku限购的最小值).null为不限购.</summary>
        public int? LimitedBuyNum => LimitedBuyNumForThisTurn;
        /// <summary>本次限购数量(spu和sku限购的最小值).null为不限购.</summary>
        public int? LimitedBuyNumForThisTurn { get; set; }
        /// <summary>sku限购数量.null为不限购.</summary>
        public int? SkuLimitedBuyNum { get; set; }
        /// <summary>spu限购数量.null为不限购.</summary>
        public int? SpuLimitedBuyNum { get; set; }

        /// <summary>
        /// course-type 1=课程 2=好物
        /// </summary>
        public int Type { get; set; }
        /// <summary>商品图</summary>
        public string? Cover { get; set; } = default;

        public bool IsValid { get; set; } = true;

        [JsonIgnore]
        public Course? _Course { get; set; } = null;

        /// <summary>成本价</summary> 
		public decimal? Costprice { get; set; }
        /// <summary>货号</summary> 
        public string? ArticleNo { get; set; }

        /// <summary>供应商id</summary> 
        public Guid? SupplierId { get; set; }

        /// <summary>
        /// 积分兑换信息
        /// </summary>
        public PointsExchangeInfo? PointExchange { get; set; }
    }

    public class CourseGoodsPropsSmTableItemDto
    {
        /// <summary>商品id</summary>
        public Guid GoodsId { get; set; }
        /// <summary>课程id</summary>
        public Guid CourseId { get; set; }
        /// <summary>现在价格</summary>
        public decimal Price { get; set; }
        /// <summary>原始价格.可为null.</summary>
        public decimal? Origprice { get; set; }
        /// <summary>成本价.可为null.</summary>
        public decimal? Costprice { get; set; }

        /// <summary>
        /// 兑换所需积分
        /// </summary>
        public PointsExchangeInfo? PointExchange { get; set; }

        /// <summary>商品包含的属性项s(也含属性分组)</summary>
        public IEnumerable<CourseGoodsPropsSmTableItemDto_PropItem> PropItems { get; set; } = default!;

        /// <summary>库存.可为null.</summary>
        public int? Stock { get; set; }
    }
    public class PointsExchangeInfo {

        /// <summary>
        /// 兑换所需积分
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// 兑换所需价格（可能有积分加金额一起兑换的情况）
        /// </summary>
        public decimal? Price { get; set; } = 0;
    }


    [DebuggerDisplay("{ForDebug_DisplayName}")]
    public class CourseGoodsPropsSmTableItemDto_PropItem
    {
        public Guid PropGroupId { get; set; }
        public string PropGroupName { get; set; } = default!;
        public int Sort_pg { get; set; }

        /// <summary>商品包含的属性项id</summary>
        public Guid PropItemId { get; set; }
        public string PropItemName { get; set; } = default!;
        //public string PropItemCover { get; set; } = default!;
        public int Sort_i { get; set; }

        string ForDebug_DisplayName => $"/{PropGroupName}/{PropItemName}";
    }

    public class CourseGoodsPropsSmTableItem1Dto
    {
        /// <summary>商品id</summary>
        public Guid GoodsId { get; set; }
        /// <summary>课程id</summary>
        public Guid CourseId { get; set; }
        /// <summary>现在价格</summary>
        public decimal Price { get; set; }
        ///// <summary>原始价格.可为null.</summary>
        //public decimal? Origprice { get; set; }

        public Guid PropGroupId { get; set; }
        public string PropGroupName { get; set; } = default!;
        public int Sort_pg { get; set; }

        /// <summary>商品包含的属性项id</summary>
        public Guid PropItemId { get; set; }
        public string PropItemName { get; set; } = default!;
        public int Sort_i { get; set; }
    }

    public class ApiCourseGoodsSimpleInfoDto : CourseGoodsSimpleInfoDto
    {
        /// <summary>限时优惠</summary> 
        public bool IsLimitedTimeOffer => _Course?.LimitedTimeOffer ?? false;
        /// <summary>新人专享</summary> 
        public bool IsNewUserExclusive => _Course?.NewUserExclusive ?? false;
        /// <summary>
        /// 我是不是新人 true=是 false=否 null=没计算
        /// </summary> 
        public bool? MeIsNewUser { get; set; }
    }

#nullable disable
}
