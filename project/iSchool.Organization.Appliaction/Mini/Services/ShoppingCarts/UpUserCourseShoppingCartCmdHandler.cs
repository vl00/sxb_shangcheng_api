using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infras.Locks;
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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class UpUserCourseShoppingCartCmdHandler : IRequestHandler<UpUserCourseShoppingCartCmd, UpUserCourseShoppingCartCmdResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        ILock1Factory _lock1Factory;

        public UpUserCourseShoppingCartCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, ILock1Factory lock1Factory,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._lock1Factory = lock1Factory;
        }

        public async Task<UpUserCourseShoppingCartCmdResult> Handle(UpUserCourseShoppingCartCmd cmd, CancellationToken cancellation)
        {
            var result = new UpUserCourseShoppingCartCmdResult();
            await default(ValueTask);

            await using var lck = await _lock1Factory.LockAsync(
                new Lock1Option(CacheKeys.Lck_ShoppingCart.FormatWith(cmd.UserId))
                .SetExpSec(30));

            if (!lck.IsAvailable)
                throw new CustomResponseException("系统繁忙");

            // cart get goods ids
            List<CourseShoppingCartItem> cart = null;
            {
                var lsGoodsIds = new List<Guid>();
                var pis = typeof(UpCourseShoppingCartCmdAction).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var action in cmd.Actions)
                {
                    var vs = pis.Select(pi => pi.GetValue(action)).Where(_ => _ != null);
#if DEBUG
                    if (vs.Count() != 1)
                        throw new CustomResponseException("每种操作各填一种action, 多操作请分开存放到数组", 4);
#endif
                    switch (vs.FirstOrDefault())
                    {
                        case UpCourseShoppingCartCmdAction.IGoodsAction v:
                            {
                                if (v.GoodsId == default)
                                    throw new CustomResponseException("请选择商品", Consts.Err.ShoppingCart_ArgumentNoGoods);

                                lsGoodsIds?.Add(v.GoodsId);
                            }
                            break;
                        case UpCourseShoppingCartCmdAction.ClearGoodsAction _:
                        default:
                            {
                                lsGoodsIds = null;
                            }
                            break;
                    }
                }
                lsGoodsIds = lsGoodsIds?.Distinct().AsList();
                cart = await GetCartFromCache(cmd.UserId, lsGoodsIds);
                cart.RemoveAll(_ => _ == null || _ == default);
            }

            // up cart     
            //      
            var idsToUp = new List<(Guid, int)>();
            foreach (var action in cmd.Actions)
            {
                if (action.UpCounts != null)
                {
                    if ((await Do_UpCounts(result, cart, action.UpCounts)) is Guid gid)
                        idsToUp.Add((gid, 0));
                }
                else if (action.UpSelected != null)
                {
                    if ((await Do_UpSelected(result, cart, action.UpSelected)) is Guid gid)
                        idsToUp.Add((gid, 0));
                }
                else if (action.DelGoods != null)
                {
                    if ((await Do_DelGoods(result, cart, action.DelGoods)) is Guid gid)
                        idsToUp.Add((gid, -1));
                }
                else if (action.ClearGoods != null)
                {
                    await Do_ClearGoods(result, cart, action.ClearGoods);
                    idsToUp.Add((default, -2));
                }
            }

            // reset cart cache
            if (idsToUp.Count > 0)
            {
                var cartKey = CacheKeys.ShoppingCart.FormatWith(cmd.UserId);
                using var pipe = _redis.StartPipe();
                foreach (var (gid, i) in idsToUp)
                {
                    if (i >= 0)
                    {
                        if (cart.TryGetOne(out var g, _ => _.GoodsId == gid))
                            pipe.HSet(cartKey, gid.ToString(), (g).ToJsonString(camelCase: true));
                    }
                    else if (i == -1)
                    {
                        pipe.HDel(cartKey, gid.ToString());
                    }
                    else if (i == -2)
                    {
                        pipe.Del(cartKey);
                    }
                }
                //pipe.Expire(cartKey, 60 * 60 * 24 * 7);
                await pipe.EndPipeAsync();
            }

            result.Deleteds = result.Deleteds.Distinct().ToList();
            result.Count = idsToUp.Count;
            return result;
        }

        async Task<List<CourseShoppingCartItem>> GetCartFromCache(Guid userId, List<Guid> lsGoodsIds)
        {
            if (lsGoodsIds != null && lsGoodsIds.Count < 1) return new List<CourseShoppingCartItem>();
            if (lsGoodsIds != null)
            {
                var items = await _redis.HMGetAsync<CourseShoppingCartItem>(CacheKeys.ShoppingCart.FormatWith(userId), lsGoodsIds.Select(_ => _.ToString()).ToArray());
                for (var i = 0; i < items.Length; i++)
                {
                    if (items[i] == null) continue;
                    items[i].GoodsId = lsGoodsIds[i];
                }
                return items.ToList();
            }
            else // getall
            {
                var dict = await _redis.HGetAllAsync(CacheKeys.ShoppingCart.FormatWith(userId));
                return dict.Select(_ =>
                {
                    var itm = _.Value.ToObject<CourseShoppingCartItem>() ?? new CourseShoppingCartItem();
                    itm.GoodsId = Guid.Parse(_.Key);
                    return itm;
                }).ToList();
            }
        }

        /// <summary>更新(加减)商品数量</summary>
        async Task<Guid?> Do_UpCounts(UpUserCourseShoppingCartCmdResult result, List<CourseShoppingCartItem> cart, UpCourseShoppingCartCmdAction.UpCountsAction action)
        {
            await default(ValueTask);
            if (!cart.TryGetOne(out var item, _ => _.GoodsId == action.GoodsId))
                throw new CustomResponseException("商品不在购物车中", Consts.Err.ShoppingCart_NotInCart);

            if (action.Doset) item.Count = action.Count > 0 ? action.Count : throw new CustomResponseException("购物车中的商品数量应该大于0", Consts.Err.ShoppingCart_ArgumentNoGoods);
            else item.Count += action.Count;
            if (item.Count < 0) item.Count = 0;
            if (item.Count > 200) item.Count = 200;
            return item.GoodsId;
        }

        /// <summary>更新商品是否被选中</summary>
        async Task<Guid?> Do_UpSelected(UpUserCourseShoppingCartCmdResult result, List<CourseShoppingCartItem> cart, UpCourseShoppingCartCmdAction.UpSelectedAction action)
        {
            await default(ValueTask);
            if (!cart.TryGetOne(out var item, _ => _.GoodsId == action.GoodsId))
                throw new CustomResponseException("商品不在购物车中", Consts.Err.ShoppingCart_NotInCart);

            item.Selected = action.Selected;
            return item.GoodsId;
        }

        /// <summary>删除商品</summary>
        async Task<Guid?> Do_DelGoods(UpUserCourseShoppingCartCmdResult result, List<CourseShoppingCartItem> cart, UpCourseShoppingCartCmdAction.DelGoodsAction action)
        {
            await default(ValueTask);
            if (!cart.TryGetOne(out var item, _ => _.GoodsId == action.GoodsId))
                return null;
            
            cart.Remove(item);
            //result.Addeds.RemoveAll(_ => _.GoodsId == item.GoodsId);
            result.Deleteds.Add(item.GoodsId);

            return item.GoodsId;
        }

        async Task Do_ClearGoods(UpUserCourseShoppingCartCmdResult result, List<CourseShoppingCartItem> cart, UpCourseShoppingCartCmdAction.ClearGoodsAction action)
        {
            await default(ValueTask);

            //result.Addeds.RemoveAll(g => cart.Any(_ => _.GoodsId == g.GoodsId));
            result.Deleteds.AddRange(cart.Select(_ => _.GoodsId));
            cart.Clear();
        }

    } //*/
}
