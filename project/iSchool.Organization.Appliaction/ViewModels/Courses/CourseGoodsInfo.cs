using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Courses
{
    /// <summary>
    /// 商品列表的返回实体
    /// </summary>
    public class CourseGoodsInfo 
    {
        /// <summary>属性总数</summary>
        public int PropertyCount { get; set; }

        /// <summary>课程的商品集合</summary>
        public List<GoodsInfo> CourseGoods { get; set; }

        /// <summary>是否支持积分兑换</summary>
        public bool EnablePointExchange { get; set; }
        /// <summary>sku积分兑换列表</summary>
        public List<SkuPointExchangeItem> SkuPointExchanges { get; set; }
    }

    /// <summary>
    /// 商品信息实体
    /// </summary>
    public class GoodsInfo
    {
        /// <summary>课程Id</summary>
        public Guid CourseId { get; set; }

        /// <summary>商品Id</summary>
        public Guid GoodsId { get; set; }

        /// <summary>供应商id</summary> 
        public Guid? SupplierId { get; set; }

        /// <summary>属性选项集合</summary>
        public List<PropertyItemInfo> PropertyItemNames { get; set; }

        /// <summary>库存</summary>
        public int Stock { get; set; }

        /// <summary>价格</summary>
        public decimal Price { get; set; }

        /// <summary>原价格</summary>
        public decimal? OrigPrice { get; set; }

        /// <summary>限购数量</summary>
        public int? LimitedBuyNum { get; set; }

        /// <summary>是否显示（1：显示；0：不显示）</summary>
        public int Show { get; set; }

        /// <summary>图片</summary>
        public string Cover { get; set; }

        /// <summary>成本价</summary> 
		public decimal? Costprice { get; set; }

        /// <summary>货号</summary> 
        public string ArticleNo { get; set; }

        /// <summary>供应商地址id</summary> 
        public Guid? SupplieAddressId { get; set; }
        /// <summary>供应商对应的所有地址,用于修改选择同一供应商的不同地址</summary> 
        public SelectListItem[] SupplieAddresses { get; set; } = null;

        /// <summary>sku直推佣金数值</summary> 
        public decimal? CashbackValue { get; set; }
        /// <summary>sku直推佣金单位(1：%；2：元)</summary> 
        public int? CashbackType { get; set; }

        /// <summary>积分数值</summary> 
        public decimal? PointCashBackValue { get; set; }
        /// <summary>积分单位(1：%；2：元)</summary> 
        public int? PointCashBackType { get; set; }
    }

    /// <summary>商品待更新实体</summary>
    public class UpdateGoodsInfo 
    {       
        /// <summary>商品Id</summary>
        public Guid GoodsId { get; set; }

        /// <summary>*库存</summary>
        public int Stock { get; set; }

        /// <summary>*价格</summary>
        public decimal Price { get; set; }

        /// <summary>原价格</summary>
        public decimal? OrigPrice { get; set; } = null;

        /// <summary>限购数量(默认为0，不限购)</summary>
        public int LimitedBuyNum { get; set; } = 0;

        /// <summary>*是否显示（1：显示；0：不显示）</summary>
        public int Show { get; set; } = 1;

        /// <summary>图片</summary>
        public string Cover { get; set; }

        /// <summary>成本价</summary> 
		public decimal? Costprice { get; set; }

        /// <summary>货号</summary> 
        public string ArticleNo { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public Guid? SupplierId { get; set; }
        /// <summary>供应商地址</summary> 
        public Guid? SupplieAddressId { get; set; }

        /// <summary>sku直推佣金数值</summary> 
        public decimal? CashbackValue { get; set; }
        /// <summary>sku直推佣金单位(1：%；2：元)</summary> 
        public int? CashbackType { get; set; }

        /// <summary>积分数值</summary> 
        public decimal? PointCashBackValue { get; set; }
        /// <summary>积分单位(1：%；2：元)</summary> 
        public int? PointCashBackType { get; set; }
    }



    /// <summary>
    /// 商品属性实体
    /// </summary>
    public class PropertyItemInfo 
    {
        /// <summary>商品Id</summary>
        public Guid GoodsId { get; set; }

        /// <summary>属性名-选项名</summary>
        public string PropertyItemName { get; set; }

        //public string Cover { get; set; }
       
    }

    /// <summary>
    /// 属性&属性选项集合
    /// </summary>
    public class PropertyAndItems 
    {
        /// <summary>属性Id</summary>
        public Guid? PropertyId { get; set; }

        /// <summary>属性Name</summary>
        public string PropertyName { get; set; }

        public string ProItemsJson { get; set; }

        /// <summary>选项集合</summary>
        public List<ProItem> ProItems { get; set; }       

        /// <summary>操作类型(1:新增(默认显示);2:更新;3:删除;4:其他(非操作))</summary>
        public int Operation { get; set; }

        /// <summary>排序</summary>
        public int Sort { get; set; }
    }

    /// <summary>
    /// 选项实体
    /// </summary>
    public class ProItem 
    {
        /// <summary>选项Id</summary>
        public Guid ItemId { get; set; }

        /// <summary>选项Name</summary>
        public string ItemName { get; set; }
           
        /// <summary>操作类型(1:新增(默认显示);2:更新;3:删除;4:其他(非操作))</summary>
        public int Operation { get; set; }

        /// <summary>排序</summary>
        public int Sort { get; set; }
    }

    /// <summary>
    /// 商品-选项Id集合
    /// </summary>
    public class GoodsItemIds 
    {
        /// <summary>商品Id</summary>
        public Guid? GoodsId { get; set; }

        public string ItemIdsJson { get; set; }

        /// <summary>商品的选项Id集合</summary>
        public List<ItemIdModel> ItemIds { get; set; }

        /// <summary>操作类型(1:新增(默认显示);2:更新;3:删除;4:其他(非操作))</summary>
        public int Operation { get; set; } = 4;             
    }
    public class ItemIdModel 
    {
        /// <summary>选项Id</summary>
        public Guid ItemId { get; set; }
    }

    /// <summary>用于加载sku兑换积分列表</summary>
    public class SkuPointExchangeItem
    {
        /// <summary>课程Id</summary>
        public Guid CourseId { get; set; }
        /// <summary>商品Id</summary>
        public Guid GoodsId { get; set; }

        /// <summary>属性选项集合</summary>
        public List<PropertyItemInfo> PropertyItemNames { get; set; }

        /// <summary>积分</summary> 
        public int? Point { get; set; }
        /// <summary>金额</summary> 
        public decimal? Price { get; set; }

        /// <summary>是否显示</summary> 
        public bool Show { get; set; }
    }

    /// <summary>用于修改sku兑换积分列表</summary>
    public class UpdateSkuPointExchangeItem
    {
        /// <summary>商品Id</summary>
        public Guid GoodsId { get; set; }

        /// <summary>积分</summary> 
        public int? Point { get; set; }
        /// <summary>金额</summary> 
        public decimal? Price { get; set; }

        /// <summary>是否显示</summary> 
        public bool Show { get; set; }
    }
}
