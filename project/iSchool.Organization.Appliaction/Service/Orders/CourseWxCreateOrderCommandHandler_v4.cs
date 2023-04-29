using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.BgServices;
using iSchool.Domain.Modles;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.Queries;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Sxb.GenerateNo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.Service.PointsMall;

namespace iSchool.Organization.Appliaction.Services
{
    public class CourseWxCreateOrderCommandHandler_v4 : IRequestHandler<CourseWxCreateOrderCmd_v4, Res2Result<CourseWxCreateOrderCmdResult_v4>>
    {
        private const string orderno_prex = "OGC";  // 一般订单号
        private const string orderno_prexA = "OGA"; // 预订单号

        private readonly int autoExpireMinute = 30; //订单自动过期时间，单位：分钟
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IUserInfo me;
        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        private readonly CSRedisClient redis;
        private readonly IConfiguration _config;
        private readonly ISxbGenerateNo _sxbGenerate;
        private readonly NLog.ILogger log;
        private readonly ILock1Factory _lck1fay;
        private readonly BgServices.RabbitMQConnectionForPublish _rabbit;
        private readonly ICouponInfoRepository _couponInfoRepository;
        private readonly ICouponReceiveRepository _couponReceiveRepository;
        private readonly IGoodsQueries _goodsQueries;
        private readonly IPointsMallService _pointsMallService;
        public CourseWxCreateOrderCommandHandler_v4(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config, ISxbGenerateNo sxbGenerate, IUserInfo me, ILock1Factory lck1fay,
            IServiceProvider services
            , ICouponInfoRepository couponInfoRepository
            , ICouponReceiveRepository couponReceiveRepository
            , IGoodsQueries goodsQueries
            , IPointsMallService pointsMallService)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.redis = redis;
            this._config = config;
            this.me = me;
            this._sxbGenerate = sxbGenerate;
            _lck1fay = lck1fay;
            this.log = services.GetService<NLog.ILogger>();
            this._rabbit = services.GetService<BgServices.RabbitMQConnectionForPublish>();
            this._hostEnvironment = services.GetService<IHostEnvironment>();
            _couponInfoRepository = couponInfoRepository;
            _couponReceiveRepository = couponReceiveRepository;
            _goodsQueries = goodsQueries;
            _pointsMallService = pointsMallService;
        }

        public async Task<Res2Result<CourseWxCreateOrderCmdResult_v4>> Handle(CourseWxCreateOrderCmd_v4 cmd, CancellationToken cancellation)
        {
            var onend = new List<object>();
            var onerror = new List<Func<Res2Result<CourseWxCreateOrderCmdResult_v4>, Task>>();
            await using var _onend_ = new DisposableSlim<List<object>>(onend, OnDispose);
            Res2Result<CourseWxCreateOrderCmdResult_v4> r = null;
            try
            {
                r = await Handle_Core(cmd, onend, onerror, cancellation);
            }
            catch (CustomResponseException ex)
            {
                r = Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>(ex.Message, ex.ErrorCode);
            }
            if (r?.Succeed != true)
            {
                await OnError(onerror, r);
            }
            return r;
        }

        async Task<Res2Result<CourseWxCreateOrderCmdResult_v4>> Handle_Core(CourseWxCreateOrderCmd_v4 cmd,
            List<object> onend, List<Func<Res2Result<CourseWxCreateOrderCmdResult_v4>, Task>> onerror,
            CancellationToken cancellation)
        {
            var result = new CourseWxCreateOrderCmdResult_v4();
            var userId = me.UserId;
            List<(CourseGoodsSimpleInfoDto Info, GoodsItem4Order Input)> courseGoodsLs = default!;
            Domain.ChildArchives[] childArchives = default!;
            var _isbuy_course1 = false; // 是否购买网课
            var isNewUserArr = new bool?[EnumUtil.GetDescs<CourseTypeEnum>().Count()];
            // 
            var isOnRwInviteActivity = false; // 直接购买
            UserSxbUnionIDDto unionID_dto = default;
            List<(Guid GoodsId, Guid CourseId, CourseExchange CourseExchange)> rwGoods = default!;
            //
            List<OrgFreightDto> orgsFreights = default!; // 运费s
            // dovalid cmd
            if (!cancellation.IsCancellationRequested)
            {
                if (string.IsNullOrEmpty(cmd.AddressDto?.Address))
                    throw new CustomResponseException("请填写收货地址");
                if (string.IsNullOrEmpty(cmd.AddressDto?.Province) || string.IsNullOrEmpty(cmd.AddressDto?.City))
                    throw new CustomResponseException("请填写收货地址");

                if ((cmd.Goods?.Length ?? -1) < 1)
                    throw new CustomResponseException("参数错误,请选择商品");

                if (cmd.Goods.GroupBy(_ => _.GoodsId).Any(_ => _.Count() > 1))
                    throw new CustomResponseException("参数错误,多个相同商品id", Consts.Err.OrderCreate_MultiSameGoods);

                if (cmd.Ver == "v3" && cmd.Goods.Length > 1)
                    throw new CustomResponseException("参数错误,直接下单应该只有1个商品");

                cmd.Remarks ??= new Dictionary<Guid, string>();
                cmd.Orgs ??= new Dictionary<Guid, OrgItem4Order>();

                // get goods
                foreach (var goods in cmd.Goods)
                {
                    if (goods.BuyCount < 1)
                        throw new CustomResponseException("购买数量应大于0");
                    if (goods.Price < 0)
                        throw new CustomResponseException("价格应大于0");

                    // 商品
                    CourseGoodsSimpleInfoDto courseGoods = null;
                    try { courseGoods = await _mediator.Send(new CourseGoodsSimpleInfoByIdQuery { GoodsId = goods.GoodsId, AllowNotValid = true, NeedCourse = true }); } catch { }
                    if (courseGoods == null)
                        throw new CustomResponseException("非法操作", Consts.Err.CourseGoodsOffline);

                    courseGoods.SupplierId ??= Guid.Empty;
                    courseGoodsLs ??= new List<(CourseGoodsSimpleInfoDto, GoodsItem4Order)>();
                    courseGoodsLs.Add((courseGoods, goods));
                }

                // check remarks
                // check orgs 
                {
                    var sppids = courseGoodsLs.GroupBy(_ => _.Info.SupplierId ?? Guid.Empty).Select(_ => _.Key);

                    if (cmd.Remarks.Any(mk => !sppids.Contains(mk.Key)))
                        throw new CustomResponseException("参数错误,remarks参数存在没商品的供应商id", Consts.Err.OrderCreate_OrgidHasNosku);

                    if (cmd.Orgs.Any(o => o.Value == null))
                        throw new CustomResponseException("参数错误,orgs参数存在没商品的供应商id", Consts.Err.OrderCreate_OrgidHasNosku);

                    if (cmd.Orgs.Any(mk => !sppids.Contains(mk.Key)))
                        throw new CustomResponseException("参数错误,orgs参数存在没商品的供应商id", Consts.Err.OrderCreate_OrgidHasNosku);
                }

                // 每次只能购买 '1个网课 or n个好物'
                for (var __ = true; __; __ = !__)
                {
                    var c1 = courseGoodsLs.Count(_ => _.Info.Type == (int)CourseTypeEnum.Course);
                    if (c1 > 1) throw new CustomResponseException("多个网课不能一起结算", Consts.Err.OrderCreate_MultiCourse1);

                    _isbuy_course1 = c1 == 1;
                    if (!_isbuy_course1) break;

                    if (courseGoodsLs.Any(_ => _.Info.Type != (int)CourseTypeEnum.Course))
                        throw new CustomResponseException("网课不能跟好物一起结算", Consts.Err.OrderCreate_OnlyCanBuyCourse1);

                    if (courseGoodsLs[0].Input.BuyCount > 1)
                        throw new CustomResponseException("网课只能每次购买1个", Consts.Err.OrderCreate_OnlyCanBuy1);
                }

                // 下架                
                {
                    foreach (var (courseGoods, _) in courseGoodsLs)
                    {
                        if (!courseGoods.IsValid)
                        {
                            result.NotValids.Add(courseGoods);
                        }
                        else if (!courseGoods._Course.IsValid)
                        {
                            result.NotValids.Add(courseGoods);
                        }
                        else if (courseGoods._Course.Status != CourseStatusEnum.Ok.ToInt())
                        {
                            result.NotValids.Add(courseGoods);
                        }
                        else if (courseGoods._Course.LastOffShelfTime != null)
                        {
                            // 自动下架job可能未跑完...
                            var diff_sec = (DateTime.Now - courseGoods._Course.LastOffShelfTime.Value).TotalSeconds;
                            if (diff_sec >= 0 && diff_sec <= 120)
                            {
                                result.NotValids.Add(courseGoods);
                            }
                        }
                    }
                    if (result.NotValids?.Count > 0)
                    {
                        return Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>(
                            "您订单里的部分商品已下架,系统为您重新核价", Consts.Err.CourseGoodsIsOffline).SetData(result);
                    }
                }
                // (本次)限购数量
                {
                    foreach (var (courseGoods, goods) in courseGoodsLs)
                    {
                        if ((courseGoods.LimitedBuyNumForThisTurn ?? -1) > 0 && goods.BuyCount > courseGoods.LimitedBuyNumForThisTurn)
                        {
                            return Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>(
                                $"您购买的{courseGoods._Course.Title}超过限购数量,点击确定返回重新下单", Consts.Err.OrderCreate_LimitedBuyNum1);
                        }
                    }
                }
                // (本次)库存
                {
                    var zeroStock = new List<CourseGoodsSimpleInfoDto>();
                    var notEnoughStock = new List<CourseGoodsSimpleInfoDto>();
                    foreach (var (courseGoods, goods) in courseGoodsLs)
                    {
                        if (courseGoods.Stock == 0)
                            zeroStock.Add(courseGoods);
                        else if (courseGoods.Stock - goods.BuyCount < 0)
                            notEnoughStock.Add(courseGoods);
                    }
                    if (notEnoughStock.Count > 0)
                    {
                        result.NoStocks = notEnoughStock;
                        return Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>(
                            "商品库存不足,点击确定返回重新下单", Consts.Err.StockNotEnough).SetData(result);
                    }
                    if (zeroStock.Count > 0)
                    {
                        result.NoStocks = zeroStock;
                        return Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>(
                            "商品无库存,请重新刷新页面", Consts.Err.NoStock).SetData(result);
                    }
                }
                // (本次)新人专享
                {
                    foreach (var (courseGoods, goods) in courseGoodsLs)
                    {
                        if (courseGoods._Course.NewUserExclusive)
                        {
                            if (goods.BuyCount > 1)
                                throw new CustomResponseException("新人专享仅限首个,点击确定返回重新下单", Consts.Err.OrderCreate_OnlyCanBuy1);
                        }
                    }
                    for (var i = 0; i < isNewUserArr.Length; i++)
                    {
                        // 不能同时购物多个新人专享
                        var ls = courseGoodsLs.Where(_ => _.Info.Type == i + 1 && _.Info._Course.NewUserExclusive);
                        if (ls.Count() > 1)
                            throw new CustomResponseException("新人专享仅限首个,点击确定返回重新下单", Consts.Err.OrderCreate_OnlyCanBuy1);

                        var (courseGoods, _) = ls.FirstOrDefault();
                        if (courseGoods == null) continue;

                        // 后续继续判断 是否新用户
                        isNewUserArr[i] = true;
                    }
                }
                // 价格
                {
                    foreach (var (courseGoods, goods) in courseGoodsLs)
                    {
                        if (courseGoods.Price != goods.Price)
                            result.PriceChangeds.Add(courseGoods);
                    }
                    if (result.PriceChangeds?.Count > 0)
                    {
                        return Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>(
                            "购买失败,商品价格有变动,点击确定系统为您重新核价", Consts.Err.PriceChanged).SetData(result);
                    }
                }
                // 运费
                {
                    var is_freightChanged = false;
                    var rr = await _mediator.Send(new GetFreightsByRecvAddressQuery
                    {
                        SkuIds = courseGoodsLs.Select(_ => _.Info.Id).Distinct().ToArray(),
                        Province = cmd.AddressDto.Province,
                        City = cmd.AddressDto.City,
                        Area = cmd.AddressDto.Area,
                        AllowFillEmptyOrgsFreights = false,
                    });
                    // 商品的运费地区在不发货地区里
                    if (rr.BlacklistSkuIds?.Any() == true)
                    {
                        result.BlacklistSkuIds = rr.BlacklistSkuIds.AsList();
                        return Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>(
                            "本商品由于特殊原因此地区赞不发货,敬请谅解!如有疑问可咨询上学帮客服人员", Consts.Err.FreightArea_of_sku_is_in_blacklist)
                            .SetData(result);
                    }
                    orgsFreights = rr.Freights.AsList();
                    foreach (var sppid in courseGoodsLs.GroupBy(_ => _.Info.SupplierId).Select(_ => _.Key))
                    {
                        if (orgsFreights.Any(_ => _.SupplierId == sppid)) continue;
                        orgsFreights.Add(new OrgFreightDto { SupplierId = sppid ?? Guid.Empty, Freight = 0 });
                    }
                    // check if has changed
                    foreach (var item in orgsFreights)
                    {
                        if (cmd.Orgs.TryGetOne(out var org, _ => _.Key == item.SupplierId))
                        {
                            if (item.Freight == org.Value.Freight) continue;
                            is_freightChanged = true;
                            break;
                        }
                        else if (item.Freight != 0)
                        {
                            // 没传品牌的运费
                            is_freightChanged = true;
                            break;
                        }
                    }
                    if (is_freightChanged)
                    {
                        return Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>("购买失败,运费有变动,点击确定系统为您重新核价", Consts.Err.FreightChanged);
                    }
                }
                // rw邀请活动 顾问微信群拉粉丝
                // 判断商品是否是给定的隐形上架的商品, 再判断是否有资格(积分)购买
                for (var __ = true; __; __ = !__)
                {
                    var rwgids = await redis.SMembersAsync<Guid>(CacheKeys.RwInviteActivity_InvisibleOnlineCourses);
                    rwGoods = courseGoodsLs.Where(_ => rwgids.Contains(_.Info.CourseId)).Select(_ => (_.Info.Id, _.Info.CourseId, default(CourseExchange))).ToList();
                    if (rwGoods.Count < 1) break;

                    if (courseGoodsLs.Count() > 1)
                        throw new CustomResponseException($"活动商品暂时不支持与其他商品一起结算", Consts.Err.OrderCreate_RwInviteActivity_Only1sku);

                    isOnRwInviteActivity = true;

                    var sql = "select * from CourseExchange where CourseId in @CourseIds and IsValid=1";
                    var ces = (await _orgUnitOfWork.QueryAsync<CourseExchange>(sql, new { CourseIds = rwGoods.Select(_ => _.CourseId).Distinct() })).AsList();
                    if (ces.Count == 0)
                        throw new CustomResponseException("无积分配置", Consts.Err.OrderCreate_CourseExchangeIsNull);

                    foreach (var courseExchange in ces)
                    {
                        if (courseExchange.StartTime != null && DateTime.Now < courseExchange.StartTime)
                            throw new CustomResponseException("兑换活动未开始", Consts.Err.OrderCreate_CourseExchangeNotStarted);
                        if (courseExchange.EndTime != null && courseExchange.EndTime <= DateTime.Now)
                            throw new CustomResponseException("兑换活动已结束", Consts.Err.OrderCreate_CourseExchangeIsEnded);

                        _ /*var rdk*/ = (CourseExchangeTypeEnum)courseExchange.Type switch
                        {
                            CourseExchangeTypeEnum.Ty1 => CacheKeys.RwInviteActivity_InviteeBuyQualify,
                            CourseExchangeTypeEnum.Ty2 => CacheKeys.RwInviteActivity_InviterBonusPoint,
                            _ => throw new CustomResponseException("兑换活动已结束", Consts.Err.OrderCreate_CourseExchangeIsNotRwInviteActivity),
                        };
                    }
                    for (var i = 0; i < rwGoods.Count; i++)
                    {
                        var g = rwGoods[i];
                        if (!ces.TryGetOne(out var courseExchange, _ => _.CourseId == g.CourseId)) continue;
                        g.CourseExchange = courseExchange;
                        rwGoods[i] = g;
                    }

                    unionID_dto = await _mediator.Send(new GetUserSxbUnionIDQuery { UserId = userId });
                    if (unionID_dto == null)
                        throw new CustomResponseException("购买失败, 您未绑定微信", Consts.Err.OrderCreate_UserHasNoUnionID);

                    // 后续才扣积分
                }

                // 上课电话(限网课)
                if (!cmd.Ver.IsNullOrEmpty() && cmd.BeginClassMobile.IsNullOrEmpty() && _isbuy_course1)
                {
                    throw new CustomResponseException("请填写上课电话");
                }
                if (!cmd.BeginClassMobile.IsNullOrEmpty())
                {
                    if (Regex.IsMatch(cmd.BeginClassMobile, @"[^0-9\+\-\(\)]"))
                        throw new CustomResponseException("不是有效的电话号码");
                }
                else cmd.BeginClassMobile = null;

                // 孩子归档信息(限网课)
                if (cmd.ChildrenInfoIds?.Length > 0)
                {
                    var sql = $@"select * from ChildArchives where IsValid=1 and Id in @ChildrenInfoIds ";
                    childArchives = (await _orgUnitOfWork.QueryAsync<Domain.ChildArchives>(sql, new { cmd.ChildrenInfoIds })).AsArray();
                    if (cmd.ChildrenInfoIds!.Any(id => !childArchives.Select(_ => _.Id).Contains(id)))
                    {
                        throw new CustomResponseException("存在无效的孩子信息", Consts.Err.OrderCreate_ChildrenInfoIds_NotMatch);
                    }
                }
                else if (!cmd.Ver.IsNullOrEmpty() && _isbuy_course1)
                {
                    throw new CustomResponseException("至少选择一个孩子信息");
                }
                else childArchives = Array.Empty<Domain.ChildArchives>();
            }

            // 防止用户频繁操作
            if (!(await redis.SetAsync($"org:lck2:buy_course:userid_{userId}", userId, 5, RedisExistence.Nx)))
            {
                throw new CustomResponseException("请不要重复购买");
            }

            var repay = false;
            var cgLs = courseGoodsLs.GroupBy(_ => _.Info.SupplierId).Select(_ => (SupplierId: _.Key, _)).ToArray();
            var orders = new (Guid SupplierId, Order Order, OrderDetial[] Products)[cgLs.Length];
            var advanceOrderNo = $"{(orders.Length > 1 ? orderno_prexA : orderno_prex)}{_sxbGenerate.GetNumber()}";
            var advanceOrderId = Guid.NewGuid();

            #region 限购（本次+历史）
            // 网课限制电话
            for (var __ = _isbuy_course1; __; __ = !__)
            {
                var (courseGoods, goods) = courseGoodsLs[0];
                var isLimitedBuy = (!cmd.Ver.IsNullOrEmpty() && !cmd.BeginClassMobile.IsNullOrEmpty() && (courseGoods.LimitedBuyNumForThisTurn ?? 0) > 0);
                if (isLimitedBuy)
                {
                    var ck = (courseGoods.SpuLimitedBuyNum ?? 0) > 0 ? $"org:lck2:wx_buy_course:v3_BeginClassMobile_{cmd.BeginClassMobile}&courseid_{courseGoods.CourseId}"
                        : (courseGoods.SkuLimitedBuyNum ?? 0) > 0 ? $"org:lck2:wx_buy_course:v3_BeginClassMobile_{cmd.BeginClassMobile}&goodsid_{courseGoods.Id}" : null;
                    if (ck == null) continue;
                    //
                    var lck = await _lck1fay.LockAsync(new Lock1Option(ck)
                        .SetExpSec(60 * 2)
                        .SetRetry(3));
                    //
                    onend.Add(lck);
                    if (!lck.IsAvailable) throw new CustomResponseException("系统繁忙", Consts.Err.OrderCreate_LimitedBuy_LockFailed);

                    var b = await CheckIsOverThanLimitedBuy(nameof(cmd.BeginClassMobile), cmd.BeginClassMobile, goods.BuyCount,
                        (courseGoods.CourseId, courseGoods.SpuLimitedBuyNum ?? 0), (courseGoods.Id, courseGoods.SkuLimitedBuyNum ?? 0));

                    if (b.Item1 || b.Item2)
                    {
                        throw new CustomResponseException("超过限购数量,请更换新的电话号码", 
                            b.Item1 ? Consts.Err.OrderCreate_LimitedBuyNum2_spu : Consts.Err.OrderCreate_LimitedBuyNum2);
                    }
                }
            }
            // 好物限制userid
            for (var __ = !_isbuy_course1; __; __ = !__)
            {
                var limitedGoodsLs = courseGoodsLs.Where(_ => (_.Info.SpuLimitedBuyNum ?? 0) > 0 || (_.Info.SkuLimitedBuyNum ?? 0) > 0); // 0也是不限购
                if (!limitedGoodsLs.Any()) continue; // 完全不限购

                // lck spu
                foreach (var g in limitedGoodsLs.Where(_ => (_.Info.SpuLimitedBuyNum ?? 0) > 0).GroupBy(_ => _.Info.CourseId))
                {
                    var courseid = g.Key;

                    var lck = await _lck1fay.LockAsync(new Lock1Option(
                        $"org:lck2:wx_buy_course:v4_userid_{userId}&courseid_{courseid}"
                        ).SetExpSec(60 * 2)
                        .SetRetry(3));

                    onend.Add(lck);
                    if (!lck.IsAvailable) throw new CustomResponseException("系统繁忙", Consts.Err.OrderCreate_LimitedBuy_LockFailed);
                }
                // lck sku
                foreach (var g in limitedGoodsLs.Where(_ => (_.Info.SpuLimitedBuyNum ?? 0) == 0 && (_.Info.SkuLimitedBuyNum ?? 0) > 0).GroupBy(_ => _.Info.Id))
                {
                    var goodsid = g.Key;

                    var lck = await _lck1fay.LockAsync(new Lock1Option(
                        $"org:lck2:wx_buy_course:v4_userid_{userId}&goodsid_{goodsid}"
                        ).SetExpSec(60 * 2)
                        .SetRetry(3));

                    onend.Add(lck);
                    if (!lck.IsAvailable) throw new CustomResponseException("系统繁忙", Consts.Err.OrderCreate_LimitedBuy_LockFailed);
                }
                // check
                foreach (var (courseGoods, goods) in courseGoodsLs)
                {
                    var b = await CheckIsOverThanLimitedBuy(nameof(userId), userId.ToString(), goods.BuyCount, 
                        (courseGoods.CourseId, courseGoods.SpuLimitedBuyNum ?? 0), (courseGoods.Id, courseGoods.SkuLimitedBuyNum ?? 0));

                    if (b.Item1 || b.Item2)
                    {
                        throw new CustomResponseException($"您本次购买的{courseGoods._Course.Title}数量加上以前曾经购买的数量超过限购数量,点击确定返回重新下单",
                            b.Item1 ? Consts.Err.OrderCreate_LimitedBuyNum2_spu : Consts.Err.OrderCreate_LimitedBuyNum2);
                    }
                }
            }
            #endregion 限购

            // 新人专享 是否新用户
            for (var i = 0; i < isNewUserArr.Length; i++)
            {
                if (isNewUserArr[i] != true) continue;
                var (courseGoods, goods) = courseGoodsLs.Find(_ => _.Info.Type == i + 1 && _.Info._Course.NewUserExclusive);

                var lck = await _lck1fay.LockAsync(new Lock1Option(
                    CacheKeys.OrderCreate_NewUserExclusive.FormatWith(courseGoods.Type, userId)
                    ).SetExpSec(60 * 2)
                    .SetRetry(3));

                onend.Add(lck);
                if (!lck.IsAvailable) throw new CustomResponseException("系统繁忙", Consts.Err.OrderCreate_NewUserBuy_LockFailed);

                // 是否新用户
                isNewUserArr[i] = (await _mediator.Send(new UserIsCourseTypeNewBuyerQuery { UserId = userId, CourseType = (CourseTypeEnum)courseGoods.Type })).IsNewBuyer;
                var isNewUser = isNewUserArr[i].Value;
                if (!isNewUser) throw new CustomResponseException("下单失败,你不符合本次购买条件", Consts.Err.OrderCreate_NewUserExclusiveAndOldUser);

                // 不能产生2个新人专享的待支付单                
                var b = await redis.SetAsync(CacheKeys.UnpaidOrderOfNewUserExclusive.FormatWith(courseGoods.Type, userId), $"{advanceOrderNo}_{advanceOrderId:n}", 60 * 60, RedisExistence.Nx);
                if (!b)
                {
                    //...
                    throw new CustomResponseException("您的未支付订单已包含新用户专享商品，点击确定返回重新下单！", Consts.Err.OrderCreate_NewUserExclusiveNotAllowMuitlUnpaidOrder);
                }
                else
                {
                    onerror.Add(_ => redis.LockExReleaseAsync(CacheKeys.UnpaidOrderOfNewUserExclusive.FormatWith(courseGoods.Type, userId), $"{advanceOrderNo}_{advanceOrderId:n}"));
                }
            }

            // 优先预锁上级
            bool? prebindFxHead_ok = null;


            #region //-- 查询之前待支付的单,拿那个单去预先支付
            //do
            //{
            //    if (isOnRwInviteActivity) break;
            //
            //    // 现在有待支付重新支付功能...                
            //}
            //while (false);
            #endregion 之前待支付的单


            // 正常下单 - 减库存
            if (courseGoodsLs.Count > 0)
            {
                var sok = new int?[courseGoodsLs.Count];
                result.NoStocks ??= new List<CourseGoodsSimpleInfoDto>(courseGoodsLs.Count);
                result.NoStocks.Clear();
                //             
                for (var i = 0; i < courseGoodsLs.Count; i++)
                {
                    var (courseGoods, g) = courseGoodsLs[i];
                    var stockAfterBuy = -3;
                    try
                    {
                        stockAfterBuy = (await _mediator.Send(new CourseGoodsStockRequest
                        {
                            StockCmd = new GoodsStockCommand { Id = g.GoodsId, Num = g.BuyCount }
                        })).StockResult;
                    }
                    catch { }
                    if (stockAfterBuy <= -2)
                    {
                        // 没库存了
                        result.NoStocks.Add(courseGoods);
                        sok[i] = null;
                    }
                    else sok[i] = g.BuyCount;
                }
                //
                if (result.NoStocks.Count > 0)
                {
                    // 若存在没库存的商品时,需要归还之前扣减ok的商品的库存
                    for (var i = 0; i < sok.Length; i++)
                    {
                        if (sok[i] == null) continue;
                        var (_, g) = courseGoodsLs[i];
                        try
                        {
                            Debug.Assert(g.BuyCount == sok[i]);
                            await _mediator.Send(new CourseGoodsStockRequest
                            {
                                AddStock = new AddGoodsStockCommand { Id = g.GoodsId, Num = g.BuyCount, FromDBIfNotExists = false }
                            });
                        }
                        catch { }
                    }

                    return Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>(
                        "商品库存不足,点击确定返回重新下单", Consts.Err.StockNotEnough).SetData(result);
                }
            }
            //
            // rw邀请活动 顾问微信群拉粉丝 - 判断是否有资格(积分)购买
            if (isOnRwInviteActivity)
            {
                var sok = new double?[rwGoods.Count];
                result.NoRwScores ??= new List<CourseGoodsSimpleInfoDto>(rwGoods.Count);
                result.NoRwScores.Clear();
                //
                for (var i = 0; i < rwGoods.Count; i++)
                {
                    var (courseGoods, g) = courseGoodsLs.Find(_ => _.Info.Id == rwGoods[i].GoodsId);
                    if (courseGoods == null) continue;
                    //
                    var consumed = (rwGoods[i].CourseExchange.Point ?? 0d) * g.BuyCount;
                    var score = -3d;
                    try
                    {
                        score = (await _mediator.Send(new UserScoreOnRwInviteActivityArgs { UnionID = unionID_dto.UnionID }
                            .SetCourseExchangeType((CourseExchangeTypeEnum)rwGoods[i].CourseExchange.Type)
                            .PreConsume(consumed)
                            )).GetResult<double>();
                    }
                    catch { }
                    if (score <= -2d)
                    {
                        // 没积分了
                        result.NoRwScores.Add(courseGoods);
                        sok[i] = null;
                    }
                    else sok[i] = consumed;
                }
                // 
                if (result.NoRwScores.Count > 0)
                {
                    // 若存在积分不够的商品时,需要归还之前扣减ok的积分
                    for (var i = 0; i < sok.Length; i++)
                    {
                        if (sok[i] == null) continue;
                        var consumed = sok[i].Value;
                        try
                        {
                            await _mediator.Send(new UserScoreOnRwInviteActivityArgs { UnionID = unionID_dto.UnionID }
                                .SetCourseExchangeType((CourseExchangeTypeEnum)rwGoods[i].CourseExchange.Type)
                                .PreConsume(-1 * consumed));
                        }
                        catch { }
                    }

                    // 积分不够 归还库存
                    foreach (var (_, g) in courseGoodsLs)
                    {
                        try
                        {
                            await _mediator.Send(new CourseGoodsStockRequest
                            {
                                AddStock = new AddGoodsStockCommand { Id = g.GoodsId, Num = g.BuyCount, FromDBIfNotExists = false }
                            });
                        }
                        catch { }
                    }

                    return Res2Result.Fail<CourseWxCreateOrderCmdResult_v4>(
                        "用户没资格购买该商品", Consts.Err.OrderCreate_UserHasNoScoreToBuy).SetData(result);
                }
            }

            // 正常下单 //-- or 更新之前单(有更新信息)
            //LB_write2db:
            var now = DateTime.Now;
            if (true)
            {
                for (var ii = 0; ii < orders.Length; ii++)
                {
                    var (_, order, products) = orders[ii];
                    var (_sppid, cGoods) = cgLs[ii];
                    var sppid = _sppid.Value;

                    if (order == null)
                    {
                        Debug.Assert(products == null);
                        order = new Order
                        {
                            Id = orders.Length > 1 ? Guid.NewGuid() : advanceOrderId,
                            Code = orders.Length > 1 ? $"{orderno_prex}{_sxbGenerate.GetNumber()}" : advanceOrderNo,
                            Type = (byte)OrderType.BuyCourseByWx,
                        };
                        products = new OrderDetial[cGoods.Count()];
                        for (var i = 0; i < products.Length; i++)
                        {
                            products[i] = new OrderDetial
                            {
                                Id = Guid.NewGuid(),
                                Orderid = order.Id,
                            };
                        }
                    }
                    order.Status = OrderStatusV2.Unpaid.ToInt();
                    order.Paymenttime = null;
                    order.Paymenttype = (byte)(cmd.IsWechatMiniProgram == 0 ? PaymentType.Wx :
                        cmd.IsWechatMiniProgram == 1 ? PaymentType.Wx_MiniProgram :
                        cmd.IsWechatMiniProgram == 2 ? PaymentType.Wx_InH5 :
                        PaymentType.Wx);
                    order.IsValid = true;
                    order.Creator = userId;
                    order.Courseid = products.Length == 1 ? cGoods.First().Info.CourseId : (Guid?)null;
                    order.Userid = userId;
                    order.Address = cmd.AddressDto.Address;
                    order.Mobile = cmd.AddressDto.RecvMobile;
                    order.RecvProvince = cmd.AddressDto.Province;
                    order.RecvCity = cmd.AddressDto.City;
                    order.RecvArea = cmd.AddressDto.Area;
                    order.RecvPostalcode = cmd.AddressDto.Postalcode;
                    order.RecvUsername = cmd.AddressDto.RecvUsername;
                    // age
                    {
                        int? age = null;
                        if (childArchives?.Length > 0)
                        {
                            order.ChildArchivesIds = cmd.ChildrenInfoIds!.ToJsonString();
                            age = childArchives[0].BirthDate is DateTime birth ? GetAge(DateTime.Now, birth) : age;
                        }
                        else order.ChildArchivesIds = null;
                        order.Age = age;
                    }
                    order.BeginClassMobile = cmd.BeginClassMobile;
                    order.AppointmentStatus = cmd.Ver != null ? (int?)BookingCourseStatusEnum.WaitFor : null;
                    order.Remark = cmd.Remarks.GetValueEx(sppid);
                    // SourceExtend
                    {
                        var jo = cmd.Jo == null ? new JObject() : (JObject)(cmd.Jo.DeepClone());
                        // $SourceExtend._orgsFreights.supplierId
                        var f = orgsFreights.FirstOrDefault(_ => _.SupplierId == sppid);
                        if (f != null) jo.Add("_orgsFreights", JToken.Parse(f.ToJsonString(camelCase: true, ignoreNull: true)));
                        order.SourceExtend = jo.ToString();
                    }
                    // OrderDetial
                    for (var i = 0; i < products.Length; i++)
                    {
                        var (courseGoods, gds) = cGoods.ElementAtOrDefault(i);
                        if (courseGoods == null) continue;
                        var course = courseGoods._Course;
                        var org_info = await _mediator.Send(new OrgzBaseInfoQuery { OrgId = course.Orgid });
                        products[i].Productid = courseGoods.Id;
                        products[i].Courseid = course.Id;
                        products[i].Name = $"{org_info.Name}-{course.Title}"; //\n{string.Join(' ', courseGoods.PropItems.Select(_ => _.Name))}
                        products[i].Status = order.Status;
                        products[i].Number = (short)gds.BuyCount;
                        products[i].Price = courseGoods.Price;
                        products[i].Origprice = courseGoods.Price;
                        products[i].Payment = courseGoods.Price * (short)gds.BuyCount;
                        products[i].Producttype = course.Type;
                        // set goods-content
                        {
                            var ctn = new CourseGoodsOrderCtnDto();
                            ctn.Id = course.Id;
                            ctn.No = course.No;
                            ctn.Title = course.Title;
                            ctn.Subtitle = course.Subtitle;
                            ctn.Banner = courseGoods.Cover;
                            ctn.Authentication = org_info.Authentication;
                            ctn.OrgId = org_info.Id;
                            ctn.OrgNo = org_info.No;
                            ctn.OrgName = org_info.Name;
                            ctn.OrgLogo = org_info.Logo;
                            ctn.OrgDesc = org_info.Desc;
                            ctn.OrgSubdesc = org_info.Subdesc;
                            ctn.GoodsId = courseGoods.Id;
                            ctn.PropItemIds = courseGoods.PropItems.Select(_ => _.Id).ToArray();
                            ctn.PropItemNames = courseGoods.PropItems.Select(_ => _.Name).ToArray();
                            ctn.ProdType = course.Type;
                            ctn.IsNewUserExclusive = course.NewUserExclusive;
                            ctn.SupplierId = sppid;
                            ctn.Costprice = courseGoods.Costprice;
                            ctn.ArticleNo = courseGoods.ArticleNo;
                            ctn._Ver = cmd.Ver;
                            ctn._FxHeaducode = cmd.FxHeaducode;
                            ctn._prebindFxHead_ok = prebindFxHead_ok;
                            if (isOnRwInviteActivity)
                            {
                                var rwgood = rwGoods.Find(_ => _.GoodsId == courseGoods.Id);
                                ctn._RwInviteActivity = rwgood.GoodsId == default ? null : new CourseGoodsOrderCtnDto.RwInviteActivity
                                {
                                    UnionID = unionID_dto.UnionID,
                                    CourseExchange = rwgood.CourseExchange,
                                    ConsumedScores = (rwgood.CourseExchange?.Point ?? 0) * gds.BuyCount,
                                };
                            }
                            products[i].Ctn = ctn.ToJsonString(camelCase: true);
                        }
                        products[i].ChildArchives = childArchives.ToJsonString(camelCase: true);
                        products[i].Remark = order.Remark;
                        // set source
                        {
                            products[i].SourceExtend = (gds.Jo ?? cmd.Jo)?.ToString();
                            if (Guid.TryParse((gds?.Jo?["eid"] ?? cmd?.Jo?["eid"])?.ToString(), out var eid))
                            {
                                products[i].SourceType = (byte)OrderCreateFromSource.SchoolFromWx;
                                products[i].SourceId = eid;
                            }
                            else if (!string.IsNullOrEmpty((gds?.Jo?["surl"] ?? cmd?.Jo?["surl"])?.ToString()))
                            {
                                products[i].SourceType = (byte)OrderCreateFromSource.Other;
                            }
                        }
                    }
                    //
                    order.AdvanceOrderId = advanceOrderId;
                    order.AdvanceOrderNo = advanceOrderNo;
                    order.CreateTime = now;
                    order.Payment = products.Sum(_ => _.Price * _.Number);
                    order.Freight = cmd.Orgs.GetValueEx(sppid)?.Freight ?? 0m;
                    order.Totalpayment = (order.Payment ?? 0m) + (order.Freight ?? 0m);
                    //
                    orders[ii] = (sppid, order, products);
                }


                if (cmd.CouponReceiveId != null)
                {
                    //锁定预使用优惠券
                    await UseCoupon(cmd.CouponReceiveId.Value, userId, advanceOrderId, orders);
                }
                if (cmd.CouponReceiveId == null && cmd.PayPoints.GetValueOrDefault()>0)
                {
                    //积分兑换不能和优惠券一起使用
                    var pointsInfo = await  UsePoints(orders);
                    if (pointsInfo.points != cmd.PayPoints.GetValueOrDefault())
                    {

                        throw new CustomResponseException("当前商品积分兑换额度已变更，请重新获取商品积分兑换信息。", Consts.Err.OrderUsePointsPayError);
                    }
                    if (pointsInfo.isPointsExchange)
                    {
                        //冻结积分，异常则中断支付
                        Guid freezeId = await _pointsMallService.FreezePoints(new Service.PointsMall.Models.FreezePointsRequest()
                        {
                            freezePoints = pointsInfo.points,
                            originId = advanceOrderId.ToString(),
                            originType = 1,
                            remark = pointsInfo.remark,
                            userId = userId.ToString()
                        });
                        await redis.SetAsync(CacheKeys.FreezeIdCacheKey(advanceOrderId), freezeId, TimeSpan.FromMinutes(autoExpireMinute * 2));
                    }
                    else
                    {
                        throw new CustomResponseException("当前商品不可使用积分兑换", Consts.Err.OrderUsePointsPayError);
                    }
   

                }



                // write_to_db
                try
                {
                    _orgUnitOfWork.BeginTransaction();
                    if (repay)
                    {
                        await _orgUnitOfWork.DbConnection.ExecuteAsync(@"
                            delete from [Order] where id in @OrderIds ;
                            delete from [OrderDetial] where orderid in @OrderIds ;
                        ", new { OrderIds = orders.Select(_ => _.Order.Id) }
                        , _orgUnitOfWork.DbTransaction);
                    }
                    foreach (var (_, order, products) in orders)
                    {
                        await _orgUnitOfWork.DbConnection.InsertAsync(order, _orgUnitOfWork.DbTransaction);
                        foreach (var product in products)
                            await _orgUnitOfWork.DbConnection.InsertAsync(product, _orgUnitOfWork.DbTransaction);
                    }
                    // 支付成功后才更新db里的库存.
                    _orgUnitOfWork.CommitChanges();


                }
                catch (Exception ex)
                {
                    _orgUnitOfWork.SafeRollback();
                     try {
                        Guid freezeId = await redis.GetAsync<Guid>(CacheKeys.FreezeIdCacheKey(advanceOrderId));
                        if (freezeId != default(Guid))
                        {
                            //回滚冻结积分
                            await _pointsMallService.DeFreezePoints(freezeId, userId);
                            await redis.DelAsync(CacheKeys.FreezeIdCacheKey(advanceOrderId));
                        }
                     } catch{ }
                    if (cmd.CouponReceiveId != null)
                    {
                        //撤销对应的优惠券
                        try { await _mediator.Send(new CancelCouponCommand() { OrderId = advanceOrderId }); } catch { }

                    }
                    var msg = GetLogMsg(new { cmd, orders });
                    msg.Properties["Error"] = $"下单失败.err={ex.Message}";
                    msg.Properties["StackTrace"] = ex.StackTrace;
                    msg.Properties["ErrorCode"] = !repay ? Consts.Err.OrderCreateFailed : Consts.Err.PrevOrderUpdateFailed;
                    log.Error(msg);

                    foreach (var g in cmd.Goods)
                    {
                        var num = !repay ? g.BuyCount    // 下新单失败回归库存
                                                         //: repay && needfixedStock > 0 ? -1 * needfixedStock  // repay改单失败可能需要修正库存
                            : repay ? g.BuyCount         // repay改单失败可能需要修正库存
                            : 0;

                        if (num == 0) continue;

                        // back库存
                        try
                        {
                            await _mediator.Send(new CourseGoodsStockRequest
                            {
                                AddStock = new AddGoodsStockCommand { Id = g.GoodsId, Num = num, FromDBIfNotExists = false }
                            });
                        }
                        catch { }

                        // back积分
                        if (isOnRwInviteActivity)
                        {
                            var rwgood = rwGoods.FirstOrDefault(_ => _.GoodsId == g.GoodsId);
                            if (rwgood.GoodsId != default)
                            {
                                var args = new UserScoreOnRwInviteActivityArgs { UnionID = unionID_dto.UnionID }
                                    .SetCourseExchangeType((CourseExchangeTypeEnum)rwgood.CourseExchange.Type)
                                    .PreConsume((rwgood.CourseExchange.Point ?? 0) * num * -1);
                                try { await _mediator.Send(args); }
                                catch (Exception ex1)
                                {
                                    var msg1 = GetLogMsg(args);
                                    msg1.Properties["Error"] = $"下单失败后归还积分也失败.err={ex1.Message}";
                                    msg1.Properties["StackTrace"] = ex1.StackTrace;
                                    msg1.Properties["ErrorCode"] = Consts.Err.OrderCreate_RwInviteActivity_ErrOnRollBack;
                                    log.Error(msg1);
                                }
                            }
                        }
                    }

                    throw new CustomResponseException("系统繁忙", !repay ? Consts.Err.OrderCreateFailed : Consts.Err.PrevOrderUpdateFailed);
                }
            }

            // call 预支付       
            //LB_callpay:
            result.OrderId = advanceOrderId;
            result.OrderNo = advanceOrderNo;
            var totalpayment = orders.Sum(_ => _.Order.Totalpayment ?? 0m);
            result.Totalpayment = totalpayment;

            onerror.Clear();

            for (var __ = true; __; __ = !__)
            {
                var args_callpay = new WxPayRequest
                {
                    AddPayOrderRequest = new ApiWxAddPayOrderRequest
                    {
                        UserId = userId,
                        OrderNo = result.OrderNo,
                        OrderId = result.OrderId!.Value,
                        //OrderType = 3,
                        OrderStatus = orders[0].Order.Status,
                        TotalAmount = totalpayment,
                        PayAmount = totalpayment,
                        //DiscountAmount = 0,
                        //RefundAmount = 0,
                        Remark = $"上学帮商城订单-订单尾号{result.OrderNo[^6..]}",
                        OpenId = cmd.OpenId,
                        //System = 2,
                        Attach = $"from=org&od={result.OrderId?.ToString("n")}",

                        OrderByProducts = orders.SelectMany(_ => _.Products).SelectMany(product => OrderHelper.OrderDetailSpread2ApiOrderByProducts(product, result.OrderId!.Value))
                        .Union(orders.Where(_ => _.Order.AdvanceOrderId != default && (_.Order.Freight ?? 0) > 0).Select(x => new ApiOrderByProduct
                        {
                            AdvanceOrderId = x.Order.AdvanceOrderId!.Value,
                            OrderId = x.Order.Id,
                            Amount = x.Order.Freight!.Value,
                            Price = x.Order.Freight!.Value,
                            BuyNum = 1,
                            ProductType = 3,
                        })).ToArray(),
                        /* SubOrders = orders.Select(x => 
                        {
                            var order = x.Order;
                            return new ApiSubOrder
                            {
                                UserId = order.Userid,
                                OrderId = order.Id,
                                OrderNo = order.Code,
                                OrderStatus = order.Status,
                                TotalAmount = order.Totalpayment ?? 0,
                                PayAmount = order.Totalpayment ?? 0,
                                //DiscountAmount = 0,
                                Remark = $"上学帮商城订单-订单尾号{order.Code[^6..]}",
                                FreightFee = order.Freight ?? 0,
                                TradeNo = advanceOrderNo,
                            };
                        }).ToArray(), */
                        IsRepay = repay ? 1 : 0,
                        IsWechatMiniProgram = cmd.IsWechatMiniProgram,
                        AppId = cmd.AppId,
                        NoNeedPay = (totalpayment == 0 ? 1 : 0),

                        // 用于测试 wx支付2min超时
                        //OrderExpireTime = now.AddMinutes(2),
                        // 正式30min
                        OrderExpireTime = now.AddMinutes(30),

                        FreightFee = orgsFreights.Sum(_ => _.Freight),
                    }
                };
                try
                {
                    var rkey = $"log.org.{_hostEnvironment.EnvironmentName}.ordercreate.ApiWxAddPayOrderRequest";
                    using var channel = _rabbit.OpenChannel();
                    channel.ConfirmPublish(exchange: "amq.topic", routingKey: rkey, timeout: TimeSpan.FromSeconds(1),
                        body: Encoding.UTF8.GetBytes(args_callpay.ToJsonString(camelCase: true)));
                }
                catch { }

                // 新支付是跳到另一个小程序支付
                if (cmd.IsOnlyCreateOrder)
                    break;

                // call预支付
                try { result.WxPayResult = (await _mediator.Send(args_callpay)).AddPayOrderResponse; }
                catch (Exception ex)
                {
                    var msg = GetLogMsg(args_callpay);
                    msg.Properties["Error"] = $"下单后调用支付平台(wxpay)失败.err={ex.Message}";
                    msg.Properties["StackTrace"] = ex.StackTrace;
                    msg.Properties["ErrorCode"] = Consts.Err.CallPaidApiError;
                    log.Error(msg);

                    // 失败等到订单过期回归库存  //-- or 下次下单会用回此单,库存在以后处理
                    throw new CustomResponseException("系统繁忙", Consts.Err.CallPaidApiError);
                }
            }

            // 预支付成功
            result.PollId = $"{result.OrderId?.ToString("n")}";
            // 0元支付 直接变成已支付
            if (result.WxPayResult != null || totalpayment == 0)
            {
                var args_poll = new PollCallRequest
                {
                    PreSetCmd = new PollPreSetCommand
                    {
                        Id = CacheKeys.OrderPoll_wxpay_order.FormatWith(result.PollId),
                        ResultType = typeof(WxPayOkOrderDto).FullName,
                        ResultStr = (new WxPayOkOrderDto
                        {
                            OrderId = result.OrderId!.Value,
                            OrderNo = result.OrderNo,
                            AdvanceOrderId = orders.Length > 1 ? advanceOrderId : (Guid?)null,
                            AdvanceOrderNo = result.OrderNo,
                            UserPayTime = null,
                            UserId = userId,
                            Paymoney = totalpayment,
                            OrderType = OrderType.BuyCourseByWx.ToInt(),
                            //BuyAmount = orders.Length == 1 ? orders[0].Products.Sum(_ => _.Number) : default,
                            //CourseId = courseGoodsLs.Count == 1 ? courseGoodsLs[0].Info.CourseId : default,
                            //GoodsId = courseGoodsLs.Count == 1 ? courseGoodsLs[0].Info.Id : default,
                            //Prods = courseGoodsLs.Count == 1 ? null : courseGoodsLs.Select(c => (c.Info.Id, c.Info.CourseId, c.Input.BuyCount)).ToArray(),
                            Prods = orders.SelectMany(_ => _.Products).Select(c => (c.Id, c.Orderid, c.Productid, c.Courseid, (int)c.Number)).ToArray(),
                            FxHeaducode = cmd.FxHeaducode,
                            _Ver = cmd.Ver,
                        }).ToJsonString(camelCase: true),
                        ExpSec = 60 * 60 * 3,
                    }
                };
                try { await _mediator.Send(args_poll); }
                catch (Exception ex)
                {
                    var msg = GetLogMsg(args_poll);
                    msg.Properties["Error"] = $"下单后创建轮询缓存失败.err={ex.Message}";
                    msg.Properties["StackTrace"] = ex.StackTrace;
                    msg.Properties["ErrorCode"] = Consts.Err.PollError;
                    log.Error(msg);
                    throw new CustomResponseException("系统繁忙", Consts.Err.PollError);
                }
            }

            // 0元支付 直接变成已支付
            if (totalpayment == 0)
            {
                AsyncUtils.StartNew(new WxPayRequest
                {
                    WxPayCallback = new WxPayCallbackNotifyMessage
                    {
                        OrderId = result.OrderId!.Value,
                        PayStatus = WxPayCallbackNotifyPayStatus.Success,
                        AddTime = DateTime.Now.AddSeconds(1),
                    }
                });
            }

            // 购物车下单后清除对应在购物车里的商品
            if (cmd.Ver == "v4")
            {
                await _mediator.Send(new UpUserCourseShoppingCartCmd
                {
                    UserId = userId,
                    Actions = cmd.Goods.Select(g => new UpCourseShoppingCartCmdAction
                    {
                        DelGoods = new UpCourseShoppingCartCmdAction.DelGoodsAction { GoodsId = g.GoodsId }
                    }),
                });
            }

            result.NotValids = null;
            result.NoStocks = null;
            result.PriceChangeds = null;
            result.NoRwScores = null;
            return Res2Result.Success(result);
        }

        /// <summary>检测限购</summary>
        private async Task<(bool, bool)> CheckIsOverThanLimitedBuy(string kf, string k, int buyCount, (Guid id, int limitedBuyNum) spu, (Guid id, int limitedBuyNum) sku)
        {
            // var kf = "BeginClassMobile" : "userid"
            var sql = $@"
select sum(case when d.courseid=@spuId then d.number else 0 end)as num1, sum(case when d.productid=@skuId then d.number else 0 end)as num2
from [order] o left join OrderDetial d on o.id=d.orderid 
where o.IsValid=1 and o.[type]>={OrderType.BuyCourseByWx.ToInt()} 
and (o.[status]>100 and o.[status]<>{OrderStatusV2.RefundOk.ToInt()}) 
and o.[{kf}]=@k 
{(spu.limitedBuyNum > 0 && sku.limitedBuyNum > 0 ? "and (d.courseid=@spuId or d.productid=@skuId)"
: spu.limitedBuyNum > 0 ? "and d.courseid=@spuId"
: sku.limitedBuyNum > 0 ? "and d.productid=@skuId"
: "")}
";
            var numBuyed = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<(int, int)>(sql, new { spuId = spu.id, skuId = sku.id, k });            
            return (spu.limitedBuyNum > 0 && numBuyed.Item1 + buyCount > spu.limitedBuyNum,
                sku.limitedBuyNum > 0 && numBuyed.Item2 + buyCount > sku.limitedBuyNum);
        }

        #region old codes
//        /// <summary>检测网课限购</summary>
//        private async Task<bool> CheckIsOverThanLimitedBuyCourse1(string beginClassMobile, Guid goodsId, int buyCount, int limitedBuyNum)
//        {
//            var sql = $@"
//select sum(d.number) from [order] o 
//left join OrderDetial d on o.id=d.orderid 
//where o.IsValid=1 and o.[type]>={OrderType.BuyCourseByWx.ToInt()} 
//and (o.[status]>100 and o.[status]<>{OrderStatusV2.RefundOk.ToInt()}) 
//and d.productid=@goodsId and o.BeginClassMobile=@beginClassMobile
//";
//            var numBuyed = await _orgUnitOfWork.DbConnection.ExecuteScalarAsync<int>(sql, new { goodsId, beginClassMobile });
//            return numBuyed + buyCount > limitedBuyNum;
//        }
//        /// <summary>检测好物限购</summary>
//        private async Task<bool> CheckIsOverThanLimitedBuyGoodthing(Guid userId, Guid goodsId, int buyCount, int limitedBuyNum)
//        {
//            var sql = $@"
//select sum(d.number) from [order] o 
//left join OrderDetial d on o.id=d.orderid 
//where o.IsValid=1 and o.[type]>={OrderType.BuyCourseByWx.ToInt()} 
//and (o.[status]>100 and o.[status]<>{OrderStatusV2.RefundOk.ToInt()}) 
//and d.productid=@goodsId and o.userid=@userId
//";
//            var numBuyed = await _orgUnitOfWork.DbConnection.ExecuteScalarAsync<int>(sql, new { goodsId, userId });
//            return numBuyed + buyCount > limitedBuyNum;
//        }
        #endregion old codes


        /// <summary>
        /// 核销券事务
        /// </summary>
        /// <param name="couponReceiveId"></param>
        /// <param name="userId"></param>
        /// <param name="advanceOrderId"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        private async Task UseCoupon(Guid couponReceiveId, Guid userId, Guid advanceOrderId, (Guid SupplierId, Order Order, OrderDetial[] Products)[] orders)
        {
            await using var _lck = await _lck1fay.LockAsync(CacheKeys.Coupon_ReceiveUseLck.FormatWith(couponReceiveId), 10000);
            if (!_lck.IsAvailable) throw new Exception("系统繁忙");
            //获取能参与优惠券活动的SKU（排除掉新人专享和限时优惠的SKU）
            var skuInfos = (await _goodsQueries.GetSKUInfosAsync(orders.SelectMany(o => o.Products.Select(p => p.Productid)))).Where(s => !s.NewUserExclusive && !s.LimitedTimeOffer);

            _orgUnitOfWork.BeginTransaction();
            try
            {
                if (skuInfos == null || !skuInfos.Any()) return;
                List<OrderDiscount> orderDiscounts = new List<OrderDiscount>();
                var couponReceive = await _couponReceiveRepository.FindAsync(couponReceiveId);
                var couponInfo = await _couponInfoRepository.GetAsync(couponReceive.CouponId);
                var (res, msg) = couponReceive.CanUseCheck(userId);
                if (!res) throw new Exception(msg);
                var (canUseCouponSkuInfos, couponAmount, totalPrice) = couponInfo.WhatCanIUseInBuySKUs(skuInfos.Select(s =>
                {
                    var product = orders.SelectMany(o => o.Products.Where(p => p.Productid == s.Id)).First();
                    return new BuySKU()
                    {
                        SKUId = s.Id,
                        BrandId = s.BrandId,
                        GoodTypes = s.GoodsTypeIds,
                        Number = product.Number,
                        UnitPrice = product.Price
                    };
                }));
                if (!canUseCouponSkuInfos.Any())
                {
                    throw new Exception("当前优惠券不满足使用条件。");
                }
                IEnumerable<OrderDetial> useCouponProducts = orders.SelectMany(o => o.Products.Where(p => canUseCouponSkuInfos.Any(sku => p.Productid == sku.SKUId)));
                decimal productUnUseCouponAmountTotal = couponAmount; //记录计算完后耗用了多少优惠金额
                foreach (var product in useCouponProducts)
                {
                    //根据产品对可使用优惠券产品总价的占比计算出其优惠金额
                    decimal productCouponAmount = (Math.Ceiling(((product.Payment / totalPrice) * couponAmount) * 100)) / 100M; ; //1798 - 0.01 = 1797.99
                    if ((productUnUseCouponAmountTotal - productCouponAmount) < 0)
                        productCouponAmount = productUnUseCouponAmountTotal;
                    productUnUseCouponAmountTotal -= productCouponAmount;
                    product.Payment -= productCouponAmount;
                    product.Price = (int)((product.Payment / product.Number) * 100) / 100M;
                    orderDiscounts.Add(new OrderDiscount() { Id = Guid.NewGuid(), Type = 1, OrderId = product.Id, DiscountAmount = productCouponAmount, IsValid = true });

                }



                //小数可能产生零头，将其加到最后一个产品订单中。
                //useCouponProducts.Last().Price -= (couponAmount - productUseCouponAmountTotal);
                //重新计算一下每个订单总价格
                foreach (var (_, order, products) in orders)
                {
                    order.Payment = products.Sum(_ => _.Payment);
                    order.Totalpayment = (order.Payment ?? 0m) + (order.Freight ?? 0m);
                }
                //改变券的状态
                couponReceive.SetOrderId(advanceOrderId);
                couponReceive.SetStatus(CouponReceiveState.PreUse);
                if (!(await _couponReceiveRepository.UpdateAsync(couponReceive, nameof(couponReceive.Status), nameof(couponReceive.OrderId)))) throw new Exception("优惠券使用失败");
                foreach (var orderDisCount in orderDiscounts)
                {
                    await _orgUnitOfWork.DbConnection.InsertAsync(orderDisCount, _orgUnitOfWork.DbTransaction);
                }
                _orgUnitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                var msg = GetLogMsg(couponReceiveId);
                msg.Properties["Error"] = $"订单使用优惠券异常.err={ex.Message}";
                msg.Properties["StackTrace"] = ex.StackTrace;
                msg.Properties["ErrorCode"] = Consts.Err.OrderUseCouponError;
                log.Error(msg);
                throw new CustomResponseException("使用优惠券异常", Consts.Err.OrderUseCouponError);
            }

        }

        private async Task<( bool isPointsExchange,int points, string remark)> UsePoints((Guid SupplierId, Order Order, OrderDetial[] Products)[] orders)
        {
            var productIds = orders.SelectMany(s => s.Products.Select(p => p.Productid));
            var pointsInfos = await GetProductExchangePoints(productIds);
            string courseName = string.Empty;
            foreach (var order in orders)
            {
                foreach (var product in order.Products)
                {
                    var pointsInfo = pointsInfos.FirstOrDefault(s => s.productId == product.Productid);
                    if (pointsInfo.isPointExchange)
                    {
                        if (string.IsNullOrEmpty(courseName)) {
                            courseName = pointsInfo.title;
                        }
                        product.Payment = 0;
                        product.Point = pointsInfo.points;
                        product.Price = pointsInfo.price;
                    }

                }
            }
            int totalPoints = 0;
            foreach (var (_, order, products) in orders)
            {
                order.Payment = products.Sum(_ => _.Payment);
                order.Totalpayment = (order.Payment ?? 0m) + (order.Freight ?? 0m);
                order.TotalPoints = products.Sum(_ => _.ConsumePointsTotal());
                totalPoints += order.TotalPoints.GetValueOrDefault();
            }
            return (totalPoints > 0,totalPoints, courseName);
        }

        public async Task<List<(Guid productId, int points, decimal price,string title,bool isPointExchange)>> GetProductExchangePoints(IEnumerable<Guid> productIds)
        {
            string sql = @"  SELECT CourseGoodsExchange.GoodId productId,ISNULL(CourseGoodsExchange.Point,0) Point ,ISNULL(CourseGoodsExchange.Price,0) Price,Course.Title,Course.IsPointExchange  FROM CourseGoodsExchange
  JOIN CourseGoods ON CourseGoods.Id = CourseGoodsExchange.GoodId
  JOIN Course ON Course.id = CourseGoodsExchange.CourseId 
 WHERE Course.IsValid =1 AND CourseGoodsExchange.Show =1 AND CourseGoodsExchange.IsValid =1 AND CourseGoodsExchange.GoodId  IN @productIds";
            var res = await _orgUnitOfWork.QueryAsync(sql, new { productIds });
            return res.Select(s => ((Guid)s.productId, (int)s.Point, (decimal)s.Price,(string)s.Title,(bool)s.IsPointExchange)).ToList();
        }


        NLog.LogEventInfo GetLogMsg(object paramsObj = null)
        {
            var msg = new NLog.LogEventInfo();
            msg.Properties["Time"] = DateTime.Now.ToMillisecondString();
            msg.SetClass(this.GetType().Name);
            msg.Properties["Caption"] = "wx购买课程v4";
            msg.Properties["UserId"] = me.UserId;
            msg.Properties["Level"] = "Error";
            if (paramsObj is string str) msg.Properties["Params"] = str;
            else if (paramsObj != null) msg.Properties["Params"] = (paramsObj).ToJsonString(camelCase: true);
            //msg.Properties["Error"] = $"检测敏感词意外失败.网络异常.err={ex.Message}";
            //msg.Properties["StackTrace"] = ex.StackTrace;
            //msg.Properties["ErrorCode"] = 3;
            return msg;
        }

        static int GetAge(DateTime now, DateTime birth)
        {
            var b = now.Month > birth.Month
                || (now.Month == birth.Month && now.Day >= birth.Day)
                || false;

            return b ? (now.Year - birth.Year) : (now.Year - birth.Year - 1);
        }



        static async Task OnDispose(List<object> ls)
        {
            foreach (var o in ls)
            {
                switch (o)
                {
                    case IDisposable _d:
                        try { _d.Dispose(); } catch { }
                        break;
                    case IAsyncDisposable _ad:
                        try { await _ad.DisposeAsync(); } catch { }
                        break;
                }
            }
        }

        static async Task OnError<T>(List<Func<Res2Result<T>, Task>> ls, Res2Result<T> r)
        {
            foreach (var f in ls)
            {
                try
                {
                    var t = f(r);
                    if (t != null) await t;
                }
                catch { }
            }
        }
    }
}
