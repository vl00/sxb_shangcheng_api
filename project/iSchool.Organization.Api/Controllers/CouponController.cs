using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using iSchool.Organization.Domain.Security;
using iSchool.Organization.Appliaction.Queries;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Infras.Locks;
using iSchool.Organization.Domain;
using Microsoft.AspNetCore.Authorization;
using iSchool.Organization.Appliaction.Queries.Models;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using Sxb.DelayTask.Abstraction;
using iSchool.Organization.Appliaction.DelayTasks.Order;

namespace iSchool.Organization.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        IMediator _mediator;

        IGoodsQueries _goodsQueries;
        ICouponQueries _couponQueries;
        public CouponController(IMediator mediator, ICouponQueries couponQueries, IGoodsQueries goodsQueries)
        {
            _mediator = mediator;
            _couponQueries = couponQueries;
            _goodsQueries = goodsQueries;
        }


        [HttpPost]
        [Authorize]
        public async Task<ResponseResult> Receive(CreateCouponReceiveCommand command)
        {
            var userInfo = HttpContext.RequestServices.GetService<IUserInfo>();
            try
            {
                command.UserId = userInfo.UserId;
                command.OriginType = CouponReceiveOriginType.SelfReceive;
                var couponReceive = await _mediator.Send(command);
                return ResponseResult.Success("OK");
            }
            catch (Exception ex)
            {
                return ResponseResult.Failed(ex.Message);
            }

        }


        [HttpPost]
        [Authorize]
        public async Task<ResponseResult> BatchReceive(List<CreateCouponReceiveCommand> commands)
        {
            var userInfo = HttpContext.RequestServices.GetService<IUserInfo>();
            List<dynamic> results = new List<dynamic>();
            foreach (var command in commands)
            {
                try
                {
                    command.UserId = userInfo.UserId;
                    command.OriginType = CouponReceiveOriginType.SelfReceive;
                    var couponReceive = await _mediator.Send(command);
                    results.Add(new { CouponId = command.CouponId, Msg = "OK" });
                }
                catch (Exception ex)
                {
                    results.Add(new { CouponId = command.CouponId, Msg = ex.Message });
                }

            }
            return ResponseResult.Success(results);



        }


        /// <summary>
        /// 商品优惠券
        /// </summary>
        /// <param name="goodsId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<ResponseResult> GoodsCoupons(Guid goodsId, int offset = 0, int limit = 20)
        {
            var userInfo = HttpContext.RequestServices.GetService<IUserInfo>();
            var goodsInfo = await _goodsQueries.GetGoodsInfoAsync(goodsId);
            if (goodsInfo == null)
            {
                return ResponseResult.Failed("找不到该商品");
            }
            if (goodsInfo.NewUserExclusive || goodsInfo.LimitedTimeOffer)
            {
                return ResponseResult.Success(null, "限时/新人专享商品不参与优惠券活动。");
            }
            var couponInfoReceiveStates = await _couponQueries.GetCorrelateWithGoodsCouponsAsync(userInfo.UserId
                 , goodsInfo.SKUIds.Select(skuId => skuId.ToString())
                 , new List<string>() { goodsInfo.BrandId.ToString() }
                 , goodsInfo.GoodsTypeIds?.Select(goodsTypeId => goodsTypeId.ToString()) ?? null
                 , offset
                 , limit);
            return ResponseResult.Success(couponInfoReceiveStates);
        }

        /// <summary>
        /// 购物车里的品牌以及商品券
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<ResponseResult> GetCouponsBySkuIds(OffsetRequest<ShoppingCartBrandItem> request)
        {
            var userInfo = HttpContext.RequestServices.GetService<IUserInfo>();
            if (request.Body == null)
                return ResponseResult.Failed("参数异常。");
            List<ShoppingCartSKUItem> shoppingCartSKUItems = new List<ShoppingCartSKUItem>();
            var skuInfos = await _goodsQueries.GetSKUInfosAsync(request.Body.ShoppingCartSKUItems.Select(s => s.SKUId));
            if (request.Body.ShoppingCartSKUItems != null && request.Body.ShoppingCartSKUItems.Any())
            {
                foreach (var skuInfo in skuInfos)
                {
                    if (!skuInfo.LimitedTimeOffer && !skuInfo.NewUserExclusive)
                    {
                        shoppingCartSKUItems.Add(request.Body.ShoppingCartSKUItems.First(s => s.SKUId == skuInfo.Id));
                    }
                }
            }
            //排除掉不参与优惠券的商品
            request.Body.ShoppingCartSKUItems = shoppingCartSKUItems;
            var couponInfoReceiveStates = await _couponQueries.GetShoppingCartBrandCoupons(userInfo.UserId
                  , request.Body
                  , request.Offset
                  , request.Limit);
            return ResponseResult.Success(couponInfoReceiveStates);

        }


        /// <summary>
        /// 购物车里选中SKU可用券
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<ResponseResult> GetCouponsByShoppingCartSKUItems(OffsetRequest<IEnumerable<ShoppingCartSKUItem>> request)
        {
            var userInfo = HttpContext.RequestServices.GetService<IUserInfo>();
            List<ShoppingCartSKUItem> shoppingCartSKUItems = new List<ShoppingCartSKUItem>();
            if (request.Body != null && request.Body.Any())
            {
                var skuInfos = await _goodsQueries.GetSKUInfosAsync(request.Body.Select(s => s.SKUId));
                foreach (var skuInfo in skuInfos)
                {
                    if (!skuInfo.LimitedTimeOffer && !skuInfo.NewUserExclusive)
                    {
                        var shoppingCartSKUItem = request.Body.First(s => s.SKUId == skuInfo.Id);
                        shoppingCartSKUItem.BrandId = skuInfo.BrandId;
                        shoppingCartSKUItem.GoodsTypeIds = skuInfo.GoodsTypeIds;
                        shoppingCartSKUItem.UnitPrice = skuInfo.UnitPrice;
                        shoppingCartSKUItems.Add(shoppingCartSKUItem);
                    }
                }
            }
            var couponInfoReceiveStates = await _couponQueries.GetEnableCouponsByShoppingCartSKUItems(userInfo.UserId
                 , shoppingCartSKUItems
                 , request.Offset
                 , request.Limit);
            return ResponseResult.Success(new
            {
                EnableCoupon = couponInfoReceiveStates,

            });

        }

        /// <summary>
        /// 获取结算平台里的优惠券券
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        public async Task<ResponseResult> GetCouponsByCashierSKUItems(OffsetRequest<IEnumerable<ShoppingCartSKUItem>> request)
        {
            var userInfo = HttpContext.RequestServices.GetService<IUserInfo>();
            List<ShoppingCartSKUItem> shoppingCartSKUItems = new List<ShoppingCartSKUItem>();
            if (request.Body != null && request.Body.Any())
            {
                var skuInfos = await _goodsQueries.GetSKUInfosAsync(request.Body.Select(s => s.SKUId));
                if (skuInfos.Any(skuinfo => !skuinfo.LimitedTimeOffer && !skuinfo.NewUserExclusive))
                {
                    foreach (var skuInfo in skuInfos)
                    {
                        if (!skuInfo.LimitedTimeOffer && !skuInfo.NewUserExclusive)
                        {
                            var shoppingCartSKUItem = request.Body.First(s => s.SKUId == skuInfo.Id);
                            shoppingCartSKUItem.BrandId = skuInfo.BrandId;
                            shoppingCartSKUItem.GoodsTypeIds = skuInfo.GoodsTypeIds;
                            shoppingCartSKUItem.UnitPrice = skuInfo.UnitPrice;
                            shoppingCartSKUItems.Add(shoppingCartSKUItem);
                        }
                    }
                }
                else
                {
                    return ResponseResult.Success(new List<Appliaction.ViewModels.Coupon.CouponReceive>());
                }

            }
            var couponInfoReceiveStates = await _couponQueries.GetMyCouponsByCashierSKUItems((userInfo?.UserId).GetValueOrDefault(), shoppingCartSKUItems);

            return ResponseResult.Success(couponInfoReceiveStates);

        }



        /// <summary>
        /// 品牌优惠券
        /// </summary>
        /// <param name="brandId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<ResponseResult> BrandCoupons(Guid brandId, int offset = 0, int limit = 20)
        {
            var userInfo = HttpContext.RequestServices.GetService<IUserInfo>();
            var couponInfoReceiveStates = await _couponQueries.GetCouponsByBrandAsync(userInfo.UserId
                 , brandId.ToString()
                 , offset
                 , limit);
            return ResponseResult.Success(couponInfoReceiveStates);
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseResult> GetMyCoupons(int tab, int offset = 0, int limit = 20)
        {
            var userInfo = HttpContext.RequestServices.GetService<IUserInfo>();
            if (tab == 0)
            {
                var coupons = await _couponQueries.GetMyWaitUseCouponsAsync(userInfo.UserId, offset, limit);
                return ResponseResult.Success(new
                {
                    total = coupons.total,
                    data = coupons.data
                });
            }
            if (tab == 1)
            {
                var coupons = await _couponQueries.GetMyLoseEfficacyCouponsAsync(userInfo.UserId, offset, limit);
                return ResponseResult.Success(new
                {
                    total = 0,
                    data = coupons
                });
            }
            return ResponseResult.Failed(" tab error");
        }


        [HttpPost]
        public async Task<ResponseResult> SearchCouDanGoods(OffsetRequest<SearchCouponCouDanGoods> request)
        {
            var enableRangeValue = await _couponQueries.GetCouponEnableRangeValue(request.Body.CouponId, request.Body.CheckedSkuIds);
            var goods = await _goodsQueries.SearchGoods(enableRangeValue.SKUIds, enableRangeValue.BrandIds, enableRangeValue.GoodTypes, request.Body.SearchText, request.Offset, request.Limit);
            return ResponseResult.Success(goods);

        }

        [HttpPost]
        public async Task<ResponseResult> SearchCouponSpecialGoods(OffsetRequest<SearchCouponSpecialGoods> request)
        {
            var enableRangeValue = await _couponQueries.GetCouponEnableRangeValue(request.Body.CouponId);
            var goods = await _goodsQueries.SearchGoods(enableRangeValue.SKUIds, enableRangeValue.BrandIds, enableRangeValue.GoodTypes, request.Body.SearchText, request.Offset, request.Limit);
            return ResponseResult.Success(goods);

        }

        /// <summary>
        /// 优惠专区
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Task<ResponseResult> DiscountArea(int index = 1, string couresName = "", int pageIndex = 1, int pageSize = 20)
        {
            var data = _goodsQueries.GetDiscountAreaContent(index, couresName, pageIndex, pageSize);
            return Task.FromResult(ResponseResult.Success(data));
        }


    }
}
