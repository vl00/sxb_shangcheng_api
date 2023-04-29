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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class AddCourseToShoppingCartCmdHandler : IRequestHandler<AddCourseToShoppingCartCmd, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        ILock1Factory _lock1Factory;
        IMapper _mapper;

        public AddCourseToShoppingCartCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            ILock1Factory lock1Factory, IMapper mapper,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._lock1Factory = lock1Factory;
            this._mapper = mapper;
        }

        public async Task<bool> Handle(AddCourseToShoppingCartCmd cmd, CancellationToken cancellation)
        {
            if (cmd.Count < 1) throw new CustomResponseException("参数错误", Consts.Err.ShoppingCart_ArgumentNoGoods);
            if (cmd.Price < 0) throw new CustomResponseException("参数错误", Consts.Err.ShoppingCart_ArgumentNoGoods);
            await default(ValueTask);
            
            var sku = await GetSkuInfo(cmd.GoodsId);
            var cart = await GetCartFromCache(cmd.UserId);

            // 网课（简易检测）
            if (sku.Type == CourseTypeEnum.Course.ToInt())
            {
                if (cmd.Count > 1) 
                    throw new CustomResponseException("该商品为网课,只能购买1个", Consts.Err.ShoppingCart_OnlyCanBuy1);
                if (cart.Any(_ => _.GoodsId == cmd.GoodsId)) 
                    throw new CustomResponseException("添加失败,该网课已在购物车中", Consts.Err.ShoppingCart_OnlyCanBuy1);
            }
            // 下架
            if (!sku.IsValid)
            {
                throw new CustomResponseException("商品已下架,请刷新后重新添加", Consts.Err.CourseGoodsIsOffline);
            }
            if (!sku._Course.IsValid)
            {
                throw new CustomResponseException("商品已下架,请刷新后重新添加", Consts.Err.CourseOffline);
            }
            if (sku._Course.Status != CourseStatusEnum.Ok.ToInt())
            {
                throw new CustomResponseException("商品已下架,请刷新后重新添加", Consts.Err.CourseOffline);
            }
            // 限购（简易检测）
            if (sku.LimitedBuyNumForThisTurn != null)
            {
                if (cmd.Count > sku.LimitedBuyNumForThisTurn.Value)
                    throw new CustomResponseException("本次购买已超过商品限购数量", Consts.Err.OrderCreate_LimitedBuyNum1);
            }
            // check 库存（简易检测）
            if (sku.Stock - cmd.Count < 0)
            {
                throw new CustomResponseException("添加失败,商品无库存了", Consts.Err.ShoppingCart_NoStock);
            }
            // 新人专享
            if (sku.IsNewUserExclusive)
            {
                if (cmd.Count > 1)
                    throw new CustomResponseException("添加失败,新人专享仅限首个", Consts.Err.ShoppingCart_OnlyCanBuy1);
                if (cart.Any(_ => _.GoodsId == cmd.GoodsId))
                    throw new CustomResponseException("添加失败,新人专享仅限首个", Consts.Err.ShoppingCart_OnlyCanBuy1);

                // 是否新用户
                var isNewUser = (await _mediator.Send(new UserIsCourseTypeNewBuyerQuery { UserId = cmd.UserId, CourseType = (CourseTypeEnum)sku.Type })).IsNewBuyer;
                if (!isNewUser) throw new CustomResponseException("添加失败,你不符合本次购买条件", Consts.Err.ShoppingCart_NewUserExclusiveAndOldUser);
            }
            // 价格变动
            if (cmd.Price != sku.Price)
            {
                throw new CustomResponseException("商品价格有变动,请刷新后重新添加", Consts.Err.ShoppingCart_PriceChanged);
            }
            // 2021-11-04 沈叔叔要求隐形商品不能加入购物车
            if (sku._Course.IsInvisibleOnline == true)
            {
                throw new CustomResponseException("隐形商品不能加入购物车");
            }

            //
            // 加入购物车cache
            //

            await using var lck = await _lock1Factory.LockAsync(new Lock1Option(
                CacheKeys.Lck_ShoppingCart.FormatWith(cmd.UserId)
                ).SetExpSec(60));

            if (!lck.IsAvailable) throw new CustomResponseException("系统繁忙");

            // get cart
            cart = await GetCartFromCache(cmd.UserId);

            // add
            var newItem = await Do_AddGoods(cart, cmd);
            if (newItem != null)
            {
                await _redis.HSetAsync(CacheKeys.ShoppingCart.FormatWith(cmd.UserId), newItem.GoodsId.ToString(), 
                    newItem.ToJsonString(camelCase: true, ignoreNull: true));
            }

            return true;
        }

        async Task<ApiCourseGoodsSimpleInfoDto> GetSkuInfo(Guid goodsId)
        {
            try
            {
                var sku = await _mediator.Send(new CourseGoodsSimpleInfoByIdQuery { GoodsId = goodsId, AllowNotValid = true, NeedCourse = true });
                var dto = _mapper.Map<ApiCourseGoodsSimpleInfoDto>(sku);
                return dto;
            }
            catch
            {
                throw new CustomResponseException("商品已下架", Consts.Err.CourseGoodsOffline);
            }
        }

        async Task<List<CourseShoppingCartItem>> GetCartFromCache(Guid userId, List<Guid> lsGoodsIds = null)
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

        async Task<CourseShoppingCartItem> Do_AddGoods(List<CourseShoppingCartItem> cart, AddCourseToShoppingCartCmd cmd)
        {
            await default(ValueTask);
            if (cmd.Count == 0)
                return null;

            var isadd = !cart.TryGetOne(out var item, _ => _.GoodsId == cmd.GoodsId);
            if (isadd)
            {
                item = new CourseShoppingCartItem { GoodsId = cmd.GoodsId, Count = 0 };
                cart.Add(item);

                //result.Addeds.Add(item);
                //result.Deleteds.Remove(item.GoodsId);
            }
            item.Time = DateTime.Now;
            if (item.Jo == null) item.Jo = cmd.Jo;
            else if (cmd.Jo != null)
            {
                item.Jo.Merge(cmd.Jo, new JsonMergeSettings 
                {
                    MergeNullValueHandling = MergeNullValueHandling.Ignore,
                    MergeArrayHandling = MergeArrayHandling.Concat,
                    PropertyNameComparison = StringComparison.OrdinalIgnoreCase,
                });
            }
            item.Count += cmd.Count;            
            if (item.Count < 0) item.Count = 0;
            if (item.Count > 200) item.Count = 200;
            return item;
        }

        #region old code
        // <summary>添加商品 + 更新(加减)商品数量</summary>
        //async Task<(Guid?, bool)> Do_AddGoods(UpUserCourseShoppingCartCmdResult result, List<CourseShoppingCartItem> cart, UpCourseShoppingCartCmdAction.AddGoodsAction action)
        //{
        //    await default(ValueTask);
        //    if (action.Count == 0)
        //        return (null, default);

        //    var b = !cart.TryGetOne(out var item, _ => _.GoodsId == action.GoodsId);
        //    if (b)
        //    {
        //        item = new CourseShoppingCartItem { GoodsId = action.GoodsId, Count = 0, Time = DateTime.Now };
        //        cart.Add(item);

        //        result.Addeds.Add(item);
        //        result.Deleteds.Remove(item.GoodsId);
        //    }
        //    item.Count += action.Count;
        //    if (item.Count < 0) item.Count = 0;
        //    return (item.GoodsId, b);
        //}
        #endregion old code
    }
}
