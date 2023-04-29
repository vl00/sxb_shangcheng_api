using iSchool.Organization.Appliaction.Queries.Models;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Queries
{
    public interface ICouponQueries
    {
        Task<CouponInfo> GetCoupon(Guid id);


        /// <summary>
        /// 获取可用券（获取到的仅仅是通过可用范围的券，它只能代表能被领取，不代表是可使用的）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="skuIds"></param>
        /// <param name="brandIds"></param>
        /// <param name="goodTypes"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IEnumerable<CouponInfoReceiveState>> GetEnableCouponsAsync(Guid userId, IEnumerable<string> skuIds, IEnumerable<string> brandIds, IEnumerable<string> goodTypes
            , int offset = 0, int limit = 20);

        /// <summary>
        /// 获取与商品有相关性的所有优惠券（商品的SKU、品牌、好物）。
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="skuIds"></param>
        /// <param name="brandIds"></param>
        /// <param name="goodTypes"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IEnumerable<CouponInfoReceiveState>> GetCorrelateWithGoodsCouponsAsync(Guid userId, IEnumerable<string> skuIds, IEnumerable<string> brandIds, IEnumerable<string> goodTypes
         , int offset = 0, int limit = 20);

        /// <summary>
        /// 获取品牌券
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="brandId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IEnumerable<CouponInfoReceiveState>> GetCouponsByBrandAsync(Guid userId, string brandId, int offset = 0, int limit = 20);


        /// <summary>
        /// 获取用户与所领取的优惠券的大纲信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="couponId"></param>
        /// <returns></returns>
        Task<CouponReceiveSummary> GetCouponReceiveSummaryFromUserAsync(Guid userId, Guid couponId);

        /// <summary>
        /// 获取用户领取优惠券信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<CouponReceive>> GetCouponReceivesFromUserAsync(Guid userId);



        /// <summary>
        /// 根据品牌/SKU 查询相关的优惠券列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="brandItem"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IEnumerable<CouponInfoReceiveState>> GetShoppingCartBrandCoupons(Guid userId, ShoppingCartBrandItem brandItem, int offset = 0, int limit = 20);

        /// <summary>
        /// 获取可使用优惠券通过ShoppingCartSKUItems
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shoppingCartSKUItems"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IEnumerable<CouponInfoReceiveState>> GetEnableCouponsByShoppingCartSKUItems(Guid userId, IEnumerable<ShoppingCartSKUItem> shoppingCartSKUItems, int offset = 0, int limit = 20);

        /// <summary>
        /// 获取凑单优惠券通过ShoppingCartSKUItems
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shoppingCartSKUItems"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IEnumerable<BrandCouDanCoupon>> GetCouDanCouponsByShoppingCartSKUItems(Guid userId, IEnumerable<ShoppingCartBrandItem>  shoppingCartBrandItems);



        Task<(IEnumerable<CouponReceive> data, int total)> GetMyWaitUseCouponsAsync(Guid userId, int offset = 0, int limit = 20);

        Task<IEnumerable<CouponReceive>> GetMyLoseEfficacyCouponsAsync(Guid userId, int offset = 0, int limit = 20);
        Task<PaginationResult<CouponReceiveSummariesResponse>> GetCouponReceivePageAsync(CouponReceiveQueryModel queryModel);

        /// <summary>
        /// 查询结算平台的SKU
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shoppingCartSKUItems"></param>
        /// <returns></returns>
        Task<IEnumerable<CouponReceive>> GetMyCouponsByCashierSKUItems(Guid userId, IEnumerable<ShoppingCartSKUItem> shoppingCartSKUItems);


        /// <summary>
        /// 获取优惠券可用范围里的绑定SKUIDs
        /// </summary>
        /// <param name="couponId"></param>
        /// <param name="notInSkuIds">排除项</param>
        /// <returns></returns>
        Task<IEnumerable<Guid>> GetCouponBindUseSKUIds(Guid couponId, IEnumerable<Guid> notInSkuIds);

        Task<IEnumerable<OrderUseCouponInfo>> GetOrderUseCouponInfosAsync(string? advanceOrderNo, Guid? orderId);

        /// <summary>
        /// 获取可用范围的真值
        /// </summary>
        /// <param name="couponId"></param>
        /// <param name="notInSkuIdsWhenIsBind">如果是绑定使用，可以排除这些项</param>
        /// <returns></returns>
        Task<(IEnumerable<Guid> SKUIds, IEnumerable<Guid> BrandIds, IEnumerable<int> GoodTypes)> GetCouponEnableRangeValue(Guid couponId, IEnumerable<Guid> notInSkuIdsWhenIsBind = null);


        /// <summary>
        /// 获取即将过期的CouponReceives
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<CouponReceive>> GetWillExpireCouponReceives();



        /// <summary>
        /// 查询优惠券的可用范围概览
        /// </summary>
        /// <param name="couponIds"></param>
        /// <returns></returns>
        Task<IEnumerable<(Guid CouponId, IEnumerable<EnableRangeSummary> EnableRangeSummaries)>> GetEnableRangeSummarys(IEnumerable<Guid> couponIds);


        Task<CouponInfoItem> GetCouponInfoItem(Guid id);

        Task<CouponReceiveDetailResponse> GetCouponReceiveDetailAsync(Guid id);
    }
}
