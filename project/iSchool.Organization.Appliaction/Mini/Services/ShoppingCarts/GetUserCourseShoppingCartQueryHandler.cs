using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.Queries;
using iSchool.Organization.Appliaction.Queries.Models;

namespace iSchool.Organization.Appliaction.Services
{
    public class GetUserCourseShoppingCartQueryHandler : IRequestHandler<GetUserCourseShoppingCartQuery, CourseShoppingCartDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        IMapper _mapper;
        ILock1Factory _lock1Factory;
        ICouponQueries _couponQueries;
        IGoodsQueries _goodsQueries;
        public GetUserCourseShoppingCartQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            ILock1Factory lock1Factory,
            IConfiguration config, IMapper mapper
            , ICouponQueries couponQueries
            , IGoodsQueries goodsQueries)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._mapper = mapper;
            this._lock1Factory = lock1Factory;
            _couponQueries = couponQueries;
            _goodsQueries = goodsQueries;
        }

        public async Task<CourseShoppingCartDto> Handle(GetUserCourseShoppingCartQuery query, CancellationToken cancellation)
        {
            var result = new CourseShoppingCartDto { Items = new List<CourseShoppingCartGroupItemDto>() };

            CourseShoppingCartItem[] goods = default!;
            {
                await using var lck = await _lock1Factory.LockAsync(
                    new Lock1Option(CacheKeys.Lck_ShoppingCart.FormatWith(query.UserId))
                    .SetExpSec(30));

                if (!lck.IsAvailable)
                {
                    throw new CustomResponseException("系统繁忙");
                }

                // when has temps then merge
                if (query.Temps?.Any() == true)
                {
                    var lsgoods = (await GetCartFromCache(query.UserId)).ToList();
                    await LoadAndMerge(lsgoods, query);
                    goods = lsgoods.ToArray();
                }
                // when temps is empty
                else
                {
                    goods = (await GetCartFromCache(query.UserId)).AsArray();
                }
            }

            //
            // get other infos
            if (goods?.Length > 0)
            {
                var rr = await _mediator.Send(new CourseMultiGoodsSettleInfosQuery
                {
                    UseQrcode = true,
                    AllowNotValid = true,
                    Goods = goods.Select(_ => new CourseMultiGoodsSettleInfos_Sku { Id = _.GoodsId, BuyCount = _.Count }).ToArray(),
                });

                var items = rr.CourseDtos.Select(_ => _mapper.Map<CourseShoppingCartProdItemDto>(_)).ToArray();
                foreach (var item in items)
                {
                    if (!goods.TryGetOne(out var x, _ => _.GoodsId == item.GoodsId)) continue;
                    item.Selected = x.Selected;
                    item.Time = x.Time;
                    item.Jo = x.Jo;
                }

                result.Items = items.OrderByDescending(_ => _.Time)
                    .GroupBy(_ => (_.OrgInfo.Id, _.OrgInfo.Name, _.OrgInfo.Id_s))
                    .Select(x => new CourseShoppingCartGroupItemDto
                    {
                        OrgId = x.Key.Id,
                        OrgName = x.Key.Name,
                        OrgId_s = x.Key.Id_s,
                        Goods = x.OrderByDescending(_ => _.Time).ToArray(),
                    })
                    .ToList();
            }

            if (result.Items.Any())
            {
                var skuInfos = await _goodsQueries.GetSKUInfosAsync(result.Items.SelectMany(g => g.Goods?.Any() == true ? g.Goods.Select(s => s.GoodsId) : new List<Guid>()));
                var shoppingCartBrandItems = result.Items.Select(s =>
                {

                    var shoppingCartBrandItem = new ShoppingCartBrandItem() { BrandId = s.OrgId };
                    List<ShoppingCartSKUItem> shoppingCartSKUItems = new List<ShoppingCartSKUItem>();
                    if (s.Goods != null && s.Goods.Any())
                    {
                        foreach (var goods in s.Goods)
                        {
                            var skuInfo = skuInfos.FirstOrDefault(sif => sif.Id == goods.GoodsId);
                            if (skuInfo != null)
                            {
                                shoppingCartSKUItems.Add(new ShoppingCartSKUItem()
                                {
                                    BrandId = skuInfo.BrandId,
                                    GoodsTypeIds = skuInfo.GoodsTypeIds,
                                    SKUId = skuInfo.Id,
                                    UnitPrice = goods.Price,
                                    Number = goods.BuyCount
                                });
                               
                            }
                        }
                    }
                    shoppingCartBrandItem.ShoppingCartSKUItems = shoppingCartSKUItems;
                    return shoppingCartBrandItem;
                });
                if (shoppingCartBrandItems.Any())
                {
                    var shoppingCartSKUItems = shoppingCartBrandItems.SelectMany(s => (s.ShoppingCartSKUItems != null || s.ShoppingCartSKUItems.Any()) ? s.ShoppingCartSKUItems : new List<ShoppingCartSKUItem>());
                    var brandCoudanCoupons = await _couponQueries.GetCouDanCouponsByShoppingCartSKUItems(query.UserId,shoppingCartBrandItems);
                    foreach (var item in result.Items)
                    {
                        item.CouDanCoupon = brandCoudanCoupons.FirstOrDefault(s => s.BrandId == item.OrgId)?.CouDanCoupons.FirstOrDefault();
                        item.IsContainBrandCoupon =  (await _couponQueries.GetShoppingCartBrandCoupons(query.UserId, shoppingCartBrandItems.FirstOrDefault(s => s.BrandId == item.OrgId), 0, 1)).Any();

                    }
                }
            }
            return result;
        }

        async Task<CourseShoppingCartItem[]> GetCartFromCache(Guid userId)
        {
            var dict = await _redis.HGetAllAsync(CacheKeys.ShoppingCart.FormatWith(userId));
            return dict.Select(_ =>
            {
                var itm = _.Value.ToObject<CourseShoppingCartItem>() ?? new CourseShoppingCartItem();
                itm.GoodsId = Guid.Parse(_.Key);
                return itm;
            }).ToArray();
        }

        async Task LoadAndMerge(IList<CourseShoppingCartItem> lsgoods, GetUserCourseShoppingCartQuery query)
        {
            var ischanged = false;
            await default(ValueTask);
            foreach (var temp in query.Temps)
            {
                if (!lsgoods.TryGetOne(out var good, _ => _.GoodsId == temp.GoodsId))
                {
                    ischanged = true;
                    lsgoods.Add(temp);
                    continue;
                }

                // 临时也有相同id的商品, 优先用时间最晚的
                if (good.Time < temp.Time)
                {
                    good.Time = temp.Time;
                    good.Count = temp.Count;
                    good.Selected = temp.Selected;
                    ischanged = true;
                }
            }
            // reset to cache
            if (ischanged)
            {
                var k = CacheKeys.ShoppingCart.FormatWith(query.UserId);
                using var pipe = _redis.StartPipe();
                pipe.Del(k);
                foreach (var g in lsgoods)
                {
                    pipe.HSet(k, g.GoodsId.ToString(), (new { g.Time, g.Count, g.Selected }).ToJsonString(camelCase: true));
                }
                //pipe.Expire(k, 60 * 60 * 24 * 7);
                await pipe.EndPipeAsync();
            }
        }
    }
}
