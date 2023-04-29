using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.BgServices;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.IntegrationEvents;
using iSchool.Organization.Appliaction.IntegrationEvents.Events;
using iSchool.Organization.Appliaction.Mini.RequestModels.Orders;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.PointsMall;
using iSchool.Organization.Appliaction.Service.PointsMall.Models;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class OrderPayedOkEventHandler_v4 : INotificationHandler<OrderPayedOkEvent>
    {
        IServiceProvider _services;
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;                
        IConfiguration _config;
        ILock1Factory _lck1fay;
        NLog.ILogger _log;
        IHttpClientFactory _httpClientFactory;
        IHostEnvironment _hostEnvironment;
        ICouponReceiveRepository _couponReceiveRepository;
        IOrganizationIntegrationEventService _organizationIntegrationEventService;
        private readonly IPointsMallService _pointsMallService;
        ILogger<OrderPayedOkEventHandler_v4> _logger;

        public OrderPayedOkEventHandler_v4(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            ILock1Factory lck1fay, NLog.ILogger log, IHttpClientFactory httpClientFactory,
            IConfiguration config, IServiceProvider _services
            , ICouponReceiveRepository couponReceiveRepository
            , IOrganizationIntegrationEventService organizationIntegrationEventService
            , IPointsMallService pointsMallService
            , ILogger<OrderPayedOkEventHandler_v4> logger)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._lck1fay = lck1fay;
            this._log = log;
            this._httpClientFactory = httpClientFactory;
            this._services = _services;
            this._hostEnvironment = _services.GetService<IHostEnvironment>();
            _couponReceiveRepository = couponReceiveRepository;
            _organizationIntegrationEventService = organizationIntegrationEventService;
            _pointsMallService = pointsMallService;
            _logger = logger;
        }

        public async Task Handle(OrderPayedOkEvent e, CancellationToken cancellation)
        {                             
            await default(ValueTask);
            
            var pollResult = (await _mediator.Send(new PollCallRequest
            {
                Query = new PollQuery { Id = e.OrderId.ToString("n"), DelayMs = -1 },
            })).PollQryResult.Result;

            var nowx = DateTime.Now;
            var orders = await _mediator.Send(new OrderDetailSimQuery { AdvanceOrderId = e.OrderId, IgnoreCheckExpired = true, UseReadConn = true });
            if ((orders?.Orders?.Length ?? 0) < 1 || orders.Orders.Any(_ => _.OrderStatus != (int)OrderStatusV2.Paid))
            {
                _log.Info(_log.GetNLogMsg(nameof(OrderPayedOkEvent)).SetLevel("warn").SetTime(nowx)
                    .SetUserId(orders.UserId).SetError($"支付成功后的用读连接查询订单发现不是{(int)OrderStatusV2.Paid}", Consts.Err.OrderPayedOk_ReadWriteNotSync)
                    .SetParams(orders));

                orders = await _mediator.Send(new OrderDetailSimQuery { AdvanceOrderId = e.OrderId, IgnoreCheckExpired = true, UseReadConn = false });
            }
            var orderType = orders.OrderType;

            _log.Info(_log.GetNLogMsg(nameof(OrderPayedOkEvent)).SetLevel("Debug")
                .SetUserId(orders.UserId).SetContent("支付成功后的缓存pollResult")
                .SetParams(pollResult));

            switch (orderType)
            {
                case OrderType.BuyCourseByWx:
                    await Handle_BuyCourseByWx(pollResult as WxPayOkOrderDto, orders);
                    break;
            }
            //核销优惠券
            try
            {
                var couponReceive = await _couponReceiveRepository.FindFromOrderAsync(e.OrderId);
                if (couponReceive != null)
                {
                    couponReceive.SetUsedTime(DateTime.Now);
                    couponReceive.SetStatus(CouponReceiveState.Used);
                    if (!await _couponReceiveRepository.UpdateAsync(couponReceive, nameof(couponReceive.UsedTime), nameof(couponReceive.Status))) throw new Exception("更新couponReceive使用信息失败。");
                }
            } catch (Exception ex){
                _log.Error(ex, "核销优惠券发生异常。");
            }
            //扣除冻结积分
            try
            {
                Guid freezeId = await _redis.GetAsync<Guid>(CacheKeys.FreezeIdCacheKey(e.OrderId));
                if(freezeId!=default(Guid))
                {
                    await _pointsMallService.DeductFreezePoints(freezeId, orders.UserId,5);
                    await _redis.DelAsync(CacheKeys.FreezeIdCacheKey(e.OrderId));
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex, $"扣除冻结积分发生异常。advanceId={e.OrderId}");
            }

            //赠送冻结积分
            foreach (var order in orders.Orders)
            {
                foreach (var courseOrderProdItemDto in order.Prods.OfType<CourseOrderProdItemDto>())
                {
                    //积分订单不参与赠送积分，积分为冻结积分。
                    if (courseOrderProdItemDto.PointsInfo?.Points > 0)
                        continue;
                    try
                    {
                        var remark = courseOrderProdItemDto.ProductTitle;
                        int? freezePoints = await _mediator.Send(new OrderPresentedPointsQuery() { OrderDetailId = courseOrderProdItemDto.OrderDetailId });
                        if (freezePoints.GetValueOrDefault() > 0)
                        {
                            var freezeId = await _pointsMallService.AddFreezePoints(new FreezePointsRequest()
                            {
                                userId = order.UserId.ToString(),
                                freezePoints = freezePoints.Value,
                                originId = order.AdvanceOrderId.ToString(),
                                originType = 5,
                                remark = remark
                            });
                           await _redis.SetAsync(CacheKeys.PresentedFreezePointsCacheKey(courseOrderProdItemDto.OrderDetailId), freezeId, TimeSpan.FromDays(30));

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"赠送冻结积分失败，orderDetailId = {courseOrderProdItemDto.OrderDetailId}");
                    }
                }

            }
    


        }

        private async Task Handle_BuyCourseByWx(WxPayOkOrderDto pollResult, OrderDetailSimQryResult ordersEntity)
        {
            if (ordersEntity == null && pollResult == null) return;
            if ((ordersEntity?.Orders?.Length ?? 0) < 1) return;
            var fxHeaducode = pollResult?.FxHeaducode ?? ((string)(ordersEntity.Orders[0].Prods?.FirstOrDefault() as CourseOrderProdItemDto)?._ctn?["_FxHeaducode"]);
            var ver = pollResult?._Ver ?? ordersEntity.Orders[0].Prods?.FirstOrDefault()?._Ver;
            var payedTime = pollResult?.UserPayTime ?? ordersEntity.Orders[0].UserPayTime ?? default;
            await default(ValueTask);
            var cachek4del = new List<(string, string)>();
            Debugger.Break();
            var rabbit = _services.GetService<RabbitMQConnectionForPublish>();

            // 支付成功增加销量
            foreach (var order in ordersEntity.Orders)
            {
                foreach (var courseOrderProdItemDto in order.Prods.OfType<CourseOrderProdItemDto>())
                {
                    var courseId = courseOrderProdItemDto.Id;
                    var goodsId = courseOrderProdItemDto.GoodsId;
                    var sellcount = courseOrderProdItemDto.BuyCount;
                    while (1 == 1 && sellcount > 0)
                    {
                        await using var lck = await _lck1fay.LockAsync($"org:lck:goods_add_sellcount:{goodsId}");
                        if (!lck.IsAvailable) continue;
                        try
                        {
                            var sql = $@"
update [CourseGoods] set [Sellcount]=[Sellcount]+@sellcount where id=@goodsId
update [Course] set [Sellcount]=isnull([Sellcount],0)+@sellcount,ModifyDateTime=getdate() where id=@courseId
";
                            await _orgUnitOfWork.ExecuteAsync(sql, new { goodsId, courseId, sellcount });

                            cachek4del.Add((CacheKeys.CourseBaseInfo.FormatWith(courseId), null));
                        }
                        catch (Exception ex)
                        {
                            LogError(order.UserId, "支付成功增加销量", new { orderId = order.OrderId, goodsId, courseId, sellcount }, ex);
                        }
                        break;
                    }
                }
            }
            if (cachek4del.Count > 0)
            {
                await _redis.BatchDelAsync(cachek4del, 10);
                cachek4del.Clear();
            }

            // try去掉新人专享未支付cache
            for (var __ = true; __; __ = !__)
            {
                var ks = ordersEntity.Orders.SelectMany(o => o.Prods.OfType<CourseOrderProdItemDto>().Select(_ => (o.UserId, o.AdvanceOrderId, o.AdvanceOrderNo, Prod: _)))
                    .Where(_ => _.Prod.NewUserExclusive == true)
                    .Select(_ => (CacheKeys.UnpaidOrderOfNewUserExclusive.FormatWith(_.Prod.ProdType, _.UserId), $"{_.AdvanceOrderNo}_{_.AdvanceOrderId:n}"))
                    .Distinct();
                if (!ks.Any()) break;

                foreach (var (k, v) in ks)
                {
                    try { await _redis.LockExReleaseAsync(k, v); } catch { }
                }
            }

            // 拉新 加入下线
            //-- v1.8+ 预锁粉在下单的时候就拉新了, 这里是防止下单用户之前没预锁的上级
            // mp1.4+  预锁粉可以被抢
            if (true)
            {
                await _mediator.Send(new ApiDrpFxRequest
                {
                    Ctn = new ApiDrpFxRequest.BecomSecondCmd
                    {
                        UserId = ordersEntity.UserId,
                        HeadUserId = !string.IsNullOrEmpty(fxHeaducode) && Guid.TryParse(fxHeaducode, out var fxHeadUserId) ? fxHeadUserId : default,
                        CourseName = ordersEntity.Orders[0].Prods?.OfType<CourseOrderProdItemDto>().ElementAtOrDefault(0)?.Title,
                        ForceChangePrefan = 1,
                    }
                });
            }

            // (冻结)录入买课记录并计算自购返现和上级佣金
            ApiDrpFxResponse.AddFxOrderCmdResult addFxOrderCmdResult = null;
            IList<CourseDrpInfo> courseDrpinfos = default!;
            IList<CourseGoodDrpInfo> courseGoodDrpInfos = default!;
            foreach (var order in ordersEntity.Orders)
            {
                //积分支付不给佣金
                if (order.TotalPoints.GetValueOrDefault() > 0)
                    continue;
                foreach (var courseOrderProdItemDto in order.Prods.OfType<CourseOrderProdItemDto>())
                {
                    var courseId = courseOrderProdItemDto.Id;
                    var goodsId = courseOrderProdItemDto.GoodsId;
                    var sellcount = courseOrderProdItemDto.BuyCount;

                    // 分销-自购返现                
                    var courseDrpinfo = courseDrpinfos?.FirstOrDefault(_ => _.Courseid == courseId);
                    if (courseDrpinfo == null)
                    {
                        courseDrpinfo = await _mediator.Send(new GetCourseFxSimpleInfoQuery { CourseId = courseId });
                        if (courseDrpinfo != null)
                        {
                            courseDrpinfos ??= new List<CourseDrpInfo>();
                            courseDrpinfos.Add(courseDrpinfo);
                        }
                    }
                    // sku 直推佣金等等                    
                    var courseGoodDrpInfo = courseGoodDrpInfos?.FirstOrDefault(_ => _.GoodId == goodsId);
                    if (courseGoodDrpInfo == null)
                    {
                        courseGoodDrpInfo = await _mediator.Send(new GetSkuFxSimpleInfoQuery { SkuId = goodsId });
                        if (courseGoodDrpInfo != null)
                        {
                            courseGoodDrpInfos ??= new List<CourseGoodDrpInfo>();
                            courseGoodDrpInfos.Add(courseGoodDrpInfo);
                        }
                    }
                    //
                    var course = await _mediator.Send(new CourseBaseInfoQuery { CourseId = courseId, AllowNotValid = true });

                    var addFxOrderCmd = new ApiDrpFxRequest.AddFxOrderCmd();
                    addFxOrderCmd.IsMp = !ver.IsNullOrEmpty();
                    addFxOrderCmd._FxHeaducode = fxHeaducode;
                    addFxOrderCmd.UserId = order.UserId;
                    addFxOrderCmd.ObjectId = courseId;
                    addFxOrderCmd.ObjectName = courseOrderProdItemDto.Title;
                    addFxOrderCmd.ObjectImgUrl = courseOrderProdItemDto.Banner?.FirstOrDefault();
                    addFxOrderCmd.PayAmount = courseOrderProdItemDto.Price; // 不要运费, 不用乘以数量
                    addFxOrderCmd.Number = sellcount; // 购买数量
                    //
                    addFxOrderCmd.OrderId = order.OrderId;
                    addFxOrderCmd.RelationOrderNo = order.OrderNo;
                    addFxOrderCmd.ObjectExtensions = JToken.FromObject(new { goodsId, propItemNames = courseOrderProdItemDto.PropItemNames, orderDetialId = courseOrderProdItemDto.OrderDetailId, order.OrderId });
                    addFxOrderCmd.PayTime = payedTime;
                    addFxOrderCmd.IsInvisibleOnline = course.IsInvisibleOnline == true ? 1 : 0;
                    addFxOrderCmd.CourseType = courseOrderProdItemDto.ProdType;
                    //
                    addFxOrderCmd._CourseDrpInfo = courseDrpinfo;
                    addFxOrderCmd._CourseGoodDrpInfo = courseGoodDrpInfo;
                    // 奖金锁定截止日期
                    // mp1.6* 确定收货才打款

                    // 佣金 参数 不用乘以数量, 去掉2位小数后面的尾数
                    addFxOrderCmd.BonusItems = new List<ApiDrpFxRequest.BonusItemDto>();
                    // (直推)自购返现 new code
                    {
                        var cashbackType = (CashbackTypeEnum)(courseGoodDrpInfo?.CashbackType ?? 0);
                        var cashbackValue = (courseGoodDrpInfo?.CashbackValue ?? 0);
                        var bonusItem = new ApiDrpFxRequest.BonusItemDto { Type = 1 };
                        bonusItem.RateType = (int)cashbackType;
                        bonusItem.Rate = cashbackValue;
                        switch (cashbackType)
                        {
                            case CashbackTypeEnum.Percent:
                                {
                                    //bonusItem.Amount = (courseOrderProdItemDto.Price * cashbackValue / 100m);

                                    // 2021-10-29 虎叔叔应成都要求修改的
                                    var costprice = (decimal?)courseOrderProdItemDto._ctn["costprice"] ?? 0m;
                                    bonusItem.Amount = fmt_money(costprice == 0 || (courseOrderProdItemDto.Price - costprice <= 0) ? 0 : (courseOrderProdItemDto.Price - costprice) * cashbackValue / 100m, 2);
                                    bonusItem.RateType = CashbackTypeEnum.Yuan.ToInt();
                                    bonusItem.Rate = bonusItem.Amount;
                                }
                                break;
                            case CashbackTypeEnum.Yuan:
                                {
                                    //bonusItem.Amount = cashbackValue;

                                    // 2021-12-30 跟好方,沈叔叔确认 佣金设置为元的情况 没成本|成本为0|利润小于佣金设置 都是没佣金
                                    var costprice = (decimal?)courseOrderProdItemDto._ctn["costprice"] ?? 0m;
                                    var liyun = (courseOrderProdItemDto.Price - costprice);
                                    bonusItem.Amount = fmt_money(costprice == 0 || liyun <= 0 || liyun < cashbackValue ? 0 : cashbackValue, 2);
                                    bonusItem.Rate = bonusItem.Amount;
                                }
                                break;
                            default:
                                bonusItem.Amount = 0;
                                bonusItem.RateType = CashbackTypeEnum.Yuan.ToInt();
                                break;
                        }
                        addFxOrderCmd.BonusItems.Add(bonusItem);
                    }
                    // 是否勾选上级工资系数 间推奖励
                    if (courseDrpinfo?.IsBonusRate == true)
                    {
                        addFxOrderCmd.BonusItems.Add(new ApiDrpFxRequest.BonusItemDto { Type = 2 });
                    }
                    // 上线独享
                    do
                    {
                        var headExbRateType = (CashbackTypeEnum)(courseDrpinfo?.HeadFxUserExclusiveType ?? 0);
                        var rateValue = courseDrpinfo?.HeadFxUserExclusiveValue ?? 0;

                        // 没勾选上线独享
                        if (rateValue <= 0 || (courseDrpinfo?.HeadFxUserExclusiveType ?? 0) <= 0) break;

                        // has 上线独享
                        var bonusItem = new ApiDrpFxRequest.BonusItemDto { Type = 3 };
                        {
                            //bonusItem.RateType = (int)headExbRateType;
                            //bonusItem.Rate = rateValue;
                            //bonusItem.Amount = headExbRateType switch
                            //{
                            //    // 实际支付金额 乘以 这个比例 
                            //    CashbackTypeEnum.Percent => fmt_money(courseOrderProdItemDto.Price * rateValue / 100m, 2),
                            //    CashbackTypeEnum.Yuan => (rateValue),
                            //    _ => 0,
                            //};

                            // 2021-12-27 屏蔽 上线独享
                            bonusItem.RateType = CashbackTypeEnum.Yuan.ToInt();
                            bonusItem.Rate = bonusItem.Amount = 0;
                        }
                        addFxOrderCmd.BonusItems.Add(bonusItem);
                    }
                    while (false);
                    // 平级佣金
                    do
                    {
                        if ((courseDrpinfo?.PJCashbackValue ?? 0) == 0) break;
                        var rateType = (CashbackTypeEnum)(courseDrpinfo?.PJCashbackType ?? 0);
                        if (rateType != CashbackTypeEnum.Yuan) break; // 暂不考虑百分比
                        var rateValue = courseDrpinfo?.PJCashbackValue ?? 0;

                        var bonusItem = new ApiDrpFxRequest.BonusItemDto { Type = 4 };
                        bonusItem.RateType = ((int?)courseDrpinfo?.PJCashbackType ?? 0);
                        bonusItem.Rate = rateValue;
                        {
                            // 2021-10-29 虎叔叔应成都要求修改的
                            //bonusItem.Rate = bonusItem.Amount = courseOrderProdItemDto.Price >= 9 && courseOrderProdItemDto.Price < 20 ? 0.5m
                            //    : courseOrderProdItemDto.Price >= 20 ? 1m : 0m;
                            //bonusItem.RateType = CashbackTypeEnum.Yuan.ToInt();

                            // 2021-12-02 虎叔叔取消平级佣金
                            bonusItem.RateType = CashbackTypeEnum.Yuan.ToInt();
                            bonusItem.Rate = bonusItem.Amount = 0;
                        }
                        //bonusItem.Amount = rateType switch
                        //{
                        //    CashbackTypeEnum.Yuan => (rateValue * sellcount),
                        //    _ => 0,
                        //};
                        addFxOrderCmd.BonusItems.Add(bonusItem);
                    }
                    while (false);

                    try
                    {
                        var rkey = $"log.org.{_hostEnvironment.EnvironmentName}.orderpayedok.ApiDrpFxRequest.AddFxOrderCmd";
                        using var channel = rabbit.OpenChannel();
                        var prop = channel.CreateBasicProperties();
                        prop.MessageId = $"orderdetailid_{courseOrderProdItemDto.OrderDetailId}";
                        prop.CorrelationId = $"orderid_{order.OrderId}";
                        channel.ConfirmPublish(exchange: "amq.topic", routingKey: rkey, basicProperties: prop, timeout: TimeSpan.FromSeconds(2),
                            body: Encoding.UTF8.GetBytes((new ApiDrpFxRequest
                            {
                                Ctn = addFxOrderCmd
                            }).ToJsonString(camelCase: true)));

                        //addFxOrderCmdResult = (await _mediator.Send(new ApiDrpFxRequest
                        //{
                        //    Ctn = addFxOrderCmd
                        //})).Result as ApiDrpFxResponse.AddFxOrderCmdResult;
                    }
                    catch (Exception ex)
                    {
                        LogError(order.UserId, "支付成功录入买小课记录到分销系统", addFxOrderCmd, ex);
                    }
                }
            }

            // 旧活动(刚刚上线时)期间新下线会可能成为顾问
            {
                //try
                //{
                //    Debugger.Break();
                //    await _mediator.Send(new ApiDrpFxRequest
                //    {
                //        Ctn = new ApiDrpFxRequest.BecomHeadUserInHdCmd
                //        {
                //            UserId = ordersEntity.UserId,
                //        }
                //    });
                //}
                //catch (Exception ex)
                //{
                //    LogError(ordersEntity.UserId, "活动期间新下线会可能成为顾问", new { ordersEntity.UserId }, ex);
                //}
            }

            // try购买后自动发送兑换码, 订单状态变成待收货,但可能没物流信息
            if (!ver.IsNullOrEmpty())
            {
                Debugger.Break();
                foreach (var order in ordersEntity.Orders)
                {
                    foreach (var courseOrderProdItemDto in order.Prods.OfType<CourseOrderProdItemDto>())
                    {
                        var courseId = courseOrderProdItemDto.Id;
                        if (courseOrderProdItemDto.ProdType != CourseTypeEnum.Course.ToInt()) continue; // 只有课程可以自动发送兑换码
                        if (!(await CheckIsAutoSendRedeemCodeAfterBuy(courseId))) continue;

                        // 订单状态变成待收货,但可能没物流信息
                        var args = new OrgService_bg.ExchangeManager.SendDHCodeCommand
                        {
                            OrderId = order.OrderId,
                            UserId = order.UserId,
                            SendMsgMobile = order.BeginClassMobile!, //?? order.RecvMobile,
                            CourseId = courseId,
                        };
                        try
                        {
                            var r = await _mediator.Send(args);
                            if (!r.Succeed)
                                LogError(order.UserId, "购买后自动发送兑换码", args, new Exception(r.Msg), Consts.Err.OrderPayedOk_Autosendcode_Failed);  
                        }
                        catch (Exception ex)
                        {
                            LogError(order.UserId, "购买后自动发送兑换码", args, ex, Consts.Err.OrderPayedOk_Autosendcode_Upstatus);
                        }
                    }
                }
            }

            //-- 购买成功后,以后对应的种草会有获奖机会
            // mp1.6* 确认收货后才有种草机会
            if (!ver.IsNullOrEmpty())
            {
                //var startTime = DateTime.Parse(_config["AppSettings:EvltReward:StartTime"]);
                //var endTime = DateTime.Parse(_config["AppSettings:EvltReward:EndTime"]);
                //do
                //{                    
                //    if (payedTime < startTime || payedTime > endTime) break;
                //    // 后续处理 商品单价超过x元才算
                //    Debugger.Break();
                //    // 当成活动
                //    foreach (var order in ordersEntity.Orders)
                //    {
                //        // 目前网课是只能1个订单并且订单里只能1个
                //        await _mediator.Send(new PresetEvltRewardChangesCmd
                //        {
                //            OrderDetail = order,
                //            FxHeadUserId = addFxOrderCmdResult?.ParentUserId,
                //            IsFxAdviser = addFxOrderCmdResult?.IsConsulstant ?? false,
                //        });
                //    }
                //} while (false);
            }

            // (冻结)好物新人立返奖励
            for (var __ = ver != null && courseDrpinfos?.Count > 0; __; __ = !__)
            {
                var cdrpinfos = courseDrpinfos.Where(c => c.IsValid && c.NewUserRewardType != null && (c.NewUserRewardValue ?? 0) > 0).ToArray();
                if (!cdrpinfos.Any()) break;
                Debugger.Break();
                // try do
                await _mediator.Send(new NewUserRewardOfBuyGoodthingCmd { OrdersEntity = ordersEntity, CourseDrpInfos = cdrpinfos });
            }

            // 满49元 通知升级为顾问
            if (true)
            {
                AsyncUtils.StartNew(new CheckAndNotifyUserToDoFxlvupCmd { UserId = ordersEntity.UserId });
            }

			// trydo rw活动积分消耗提醒--wx客服消息
            // rw活动目前是直接购买
            if (ordersEntity.Orders?.FirstOrDefault()?.Prods.FirstOrDefault() is CourseOrderProdItemDto cprod)
            {
                AsyncUtils.StartNew(new UserScoreConsumedOnRwInviteActivityEvent 
                { 
                    UserId = ordersEntity.UserId,
                    GoodsId = cprod.GoodsId,
                });
            }

            #region //沈叔叔 签到 送网课
            //for (var __ = ordersEntity.Orders?.Length > 0; __; __ = !__)
            //{
            //    using var http = _httpClientFactory.CreateClient(string.Empty);

            //    // 签到 - 单次付款总订单金额不算运费>=25
            //    var paymoney0 = ordersEntity.Orders.Sum(_ => _.Paymoney0);
            //    if (paymoney0 >= Convert.ToDecimal(_config["AppSettings:samApis:orderpayment4usersign"]))
            //    {
            //        var openid = "";
            //        var mobile = "";
            //        try
            //        {
            //            var r = await _mediator.Send(new GetUserOpenIdQryArgs { UserId = ordersEntity.UserId });
            //            mobile = r.Item1;
            //            openid = r.Item2;
            //        }
            //        catch { }
                 

            //        await new HttpApiInvocation(_log).SetAllowLogOnDebug(true)
            //            .SetApiDesc("沈叔叔签到api")
            //            .SetUrl(_config["BaseUrls:sam"] + "/memberSign").SetMethod(HttpMethod.Post)
            //            .SetBodyByJson(new 
            //            {
            //                mobile= mobile,
            //                userid = ordersEntity.UserId,
            //                orderid = ordersEntity.AdvanceOrderId, // 预订单id
            //                openid,                           
            //            })
            //            .OnAfterResponse(async res => 
            //            {
            //                if (!res.IsSuccessStatusCode) return ResponseResult<JToken>.Failed().Set_status(res.StatusCode.ToInt());
            //                var r = JToken.Parse(await res.Content.ReadAsStringAsync());
            //                if ((int)r["status"] != 0) return ResponseResult<JToken>.Failed().Set_status((int)r["status"]).SetData(r);
            //                return ResponseResult<JToken>.Success(r);
            //            })
            //            .InvokeByAsync(http);
            //    }

            //    // 送网课 - 只要买了都可以调用
            //    {
            //        await new HttpApiInvocation(_log).SetAllowLogOnDebug(true)
            //            .SetApiDesc("沈叔叔送网课api")
            //            .SetUrl(_config["BaseUrls:sam"] + "/sendGift").SetMethod(HttpMethod.Post)
            //            .SetBodyByJson(new
            //            {
            //                userid = ordersEntity.UserId,
            //                // 订单填写的收货地址手机号
            //                mobile = ordersEntity.Orders.First().RecvMobile,
            //                // spu id
            //                productids = ordersEntity.Orders.SelectMany(_ => _.Prods.OfType<CourseOrderProdItemDto>()).Select(_ => _.Id).Distinct().ToArray(),
            //            })
            //            .OnAfterResponse(async res =>
            //            {
            //                if (!res.IsSuccessStatusCode) return ResponseResult<JToken>.Failed().Set_status(res.StatusCode.ToInt());
            //                var r = JToken.Parse(await res.Content.ReadAsStringAsync());
            //                if ((int)r["status"] != 0) return ResponseResult<JToken>.Failed().Set_status((int)r["status"]).SetData(r);
            //                return ResponseResult<JToken>.Success(r);
            //            })
            //            .InvokeByAsync(http);
            //    }
            //}
            #endregion //沈叔叔 签到 送网课




            #region 成功给王宁dalao打卡...
            if (ordersEntity.Orders?.Length > 0)
            {
                var ts = new List<Task>(ordersEntity.Orders.Length);
                foreach (var oder in ordersEntity.Orders)
                {
                    ts.Add(_services.GetService<IntegrationEvents.IOrganizationIntegrationEventService>().PublishEventAsync(new IntegrationEvents.Events.OrderPayOkIntegrationEvent
                    {
                        OrderId = oder.OrderId,
                        UserId = oder.UserId,

                    }));
                }
                try { await Task.WhenAll(ts); }
                catch (Exception ex)
                {
                    LogError(ordersEntity.UserId, "支付成功后给王宁dalao打卡报错了", ordersEntity, ex, 555);
                }
            }
            #endregion 成功给王宁dalao打卡...


 
            //公布订单支付成功集成事件
            try {
                OrdersPayOkIntegrationEvent ordersPayOkIntegrationEvent = new OrdersPayOkIntegrationEvent();
                var firstOrder = ordersEntity.Orders.First();
                ordersPayOkIntegrationEvent.UserId = firstOrder.UserId;
                ordersPayOkIntegrationEvent.AdvanceOrderId = firstOrder.AdvanceOrderId;
                ordersPayOkIntegrationEvent.AdvanceOrderNo = firstOrder.AdvanceOrderNo;
                ordersPayOkIntegrationEvent.AdvanceOrderIsPointsPay = ordersEntity.Orders.Any(o => o.TotalPoints.GetValueOrDefault() > 0);
                ordersPayOkIntegrationEvent.PaymentTime = firstOrder.UserPayTime.GetValueOrDefault();
                ordersPayOkIntegrationEvent.Orders = ordersEntity.Orders.Select(o => {
                    IntegrationEvents.Events.Order eventOrder = new IntegrationEvents.Events.Order();
                    eventOrder.Id = o.OrderId;
                    eventOrder.OrderNo = o.OrderNo;
                    eventOrder.OrderDetails = o.Prods.Select(prod =>
                    {
                        IntegrationEvents.Events.OrderDetail eventOrderDetail = new IntegrationEvents.Events.OrderDetail();
                        eventOrderDetail.Id = prod.OrderDetailId;
                        eventOrderDetail.ProductId = prod.ProductId;
                        eventOrderDetail.ProductName = prod.ProductTitle;
                        return eventOrderDetail;
                    }).ToList();
                    return eventOrder;
                }).ToList();
                await _organizationIntegrationEventService.PublishEventAsync(ordersPayOkIntegrationEvent); } 
            catch { }
        }

        /// <summary>
        /// 检查是否购买后自动发货
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        private async Task<bool> CheckIsAutoSendRedeemCodeAfterBuy(Guid courseId)
        {
            var sql = "select top 1 * from MsgTemplate where courseid=@courseId order by IsAuto desc";
            var dm = await _orgUnitOfWork.QueryFirstOrDefaultAsync<MsgTemplate>(sql, new { courseId });
            return dm?.IsAuto ?? false;
        }

        static decimal fmt_money(decimal v, int d)
        {
            if (d == 0) return decimal.Truncate(v);
            var str = v.ToString().AsSpan();
            var i = str.IndexOf('.');
            return i == -1 || (i + 1 + d) > str.Length ? v : decimal.Parse(str[..(i + 1 + d)]);
        }

        void LogError(Guid userid, string errdesc, object obj, Exception ex, int errcode = 500)
        {
            LogError(userid, errdesc, obj?.ToJsonString(camelCase: true), ex, errcode);
        }

        void LogError(Guid userid, string errdesc, string paramsStr, Exception ex, int errcode = 500)
        {
            if (_log != null)
            {
                _log.Error(_log.GetNLogMsg(nameof(OrderPayedOkEvent))
                    .SetUserId(userid)
                    .SetParams(paramsStr)
                    .SetLevel("错误")
                    .SetError(ex, errdesc, errcode));
            }
        }
    }
}
