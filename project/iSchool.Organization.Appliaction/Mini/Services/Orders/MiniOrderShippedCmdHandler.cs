using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.DelayTasks.Order;
using iSchool.Organization.Appliaction.Mini.RequestModels.Orders;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Appliaction.Service.PointsMall;
using iSchool.Organization.Appliaction.Service.PointsMall.Models;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Sxb.DelayTask.Abstraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class MiniOrderShippedCmdHandler : IRequestHandler<MiniOrderShippedCmd, bool>
    {
        private readonly IUserInfo me;
        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        private readonly CSRedisClient redis;
        private readonly IConfiguration _config;
        ILock1Factory _lock1Factory;


        public MiniOrderShippedCmdHandler(IOrgUnitOfWork orgUnitOfWork
            , CSRedisClient redis
            , IConfiguration _config
            , ILock1Factory lock1Factory
            , IMediator mediator
            , IUserInfo me)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;
            this._config = _config;
            this._lock1Factory = lock1Factory;
        }

        public async Task<bool> Handle(MiniOrderShippedCmd cmd, CancellationToken cancellation)
        {
            if (!cmd.IsFromAuto && !(await redis.SetAsync($"org:lck2:user_shipped_order:{me.UserId}", Guid.NewGuid(), 5, RedisExistence.Nx)))
            {
                throw new CustomResponseException("不要重复点击确定收货", Consts.Err.ShippedOrderTooManyTimes);
            }

            await using var _lck = await _lock1Factory.LockAsync($"org:lck:600-20:shipped_order:{cmd.OrderId}", 1000 * 60 * 10);
            if (!_lck.IsAvailable) throw new CustomResponseException("不要重复点击确定收货", Consts.Err.ShippedOrderTooManyTimes);

            var orders = await _mediator.Send(new OrderDetailSimQuery { OrderId = cmd.OrderId });
            var order = orders.Orders?.FirstOrDefault();
            if (order == null)
                throw new CustomResponseException("无效的订单", Consts.Err.OrderIsNotValid_OnShipped);

            if (order.OrderStatus <= 300 || ((OrderStatusV2)order.OrderStatus) != OrderStatusV2.Shipping)
                throw new CustomResponseException("当前订单状态不能确定收货", Consts.Err.OrderStatus_IsNot_Shipping);

            if (!cmd.IsFromAuto && order.UserId != me.UserId)
                throw new CustomResponseException("订单是别人的", Consts.Err.ShippedOrderFailed_UserNotSame);

            // update db
            List<OrderRefunds> rdfs = null;
            var now = DateTime.Now;
            try
            {
                _orgUnitOfWork.BeginTransaction();

                var sql = @"
update [OrderDetial] set [status]=@status where OrderId=@OrderId and [status]=@status0

update [Order] set [status]=@status,[ModifyDateTime]=@now,[SourceExtend]=JSON_MODIFY([SourceExtend],'$.shipped',JSON_QUERY(@str_shipped))
where id=@OrderId and [status]=@status0
";
                var i = await _orgUnitOfWork.ExecuteAsync(sql, new
                {
                    cmd.OrderId,
                    status = (int)OrderStatusV2.Shipped,
                    status0 = (int)OrderStatusV2.Shipping,
                    now,
                    str_shipped = $"{{\"time\":\"{now:yyyy-MM-dd HH:mm:ss.fff}\",\"isFromAuto\":{cmd.IsFromAuto.ToString().ToLower()}}}",
                }, _orgUnitOfWork.DbTransaction);
                if (i < 1) throw new CustomResponseException("系统繁忙", Consts.Err.ShippedOrderFailed_Status0_noteq_Shipping);

                //if (!cmd.IsFromAuto)
                {
                    // 确认收货要关闭退款申请
                    try
                    {
                        sql = $@"
select * from [OrderRefunds] where IsValid=1 and OrderId=@OrderId 
and [type] not in @type and [status] not in @status
";
                        rdfs = (await _orgUnitOfWork.DbConnection.QueryAsync<OrderRefunds>(sql, new
                        {
                            cmd.OrderId,
                            UserId = !cmd.IsFromAuto ? order.UserId.ToString() : "00111111-1111-1111-1111-111111111100",
                            cstt = (int)RefundStatusEnum.Cancel,
                            type = (new[] { RefundTypeEnum.FastRefund, RefundTypeEnum.BgRefund }).Select(_ => (int)_),
                            status = (new[]
                            {
                                RefundStatusEnum.RefundSuccess, RefundStatusEnum.ReturnSuccess,
                                RefundStatusEnum.RefundAuditFailed, RefundStatusEnum.ReturnAuditFailed, RefundStatusEnum.InspectionFailed,
                                RefundStatusEnum.Cancel, RefundStatusEnum.CancelByExpired
                            }).Select(_ => (int)_),
                        }, _orgUnitOfWork.DbTransaction)).AsList();

                        if (rdfs.Count > 0)
                        {
                            sql = $@"
update [OrderRefunds] set [status]=@cstt,[ModifyDateTime]=getdate(),Modifier=@UserId where Id in @Ids
";
                            await _orgUnitOfWork.ExecuteAsync(sql, new
                            {
                                UserId = !cmd.IsFromAuto ? order.UserId.ToString() : "00111111-1111-1111-1111-111111111100",
                                cstt = (int)RefundStatusEnum.Cancel,
                                Ids = rdfs.Select(_ => _.Id).ToArray(),
                            }, _orgUnitOfWork.DbTransaction);
                        }
                    }
                    catch
                    {
                        throw new CustomResponseException("系统繁忙", Consts.Err.ShippedOrderFailed_CancelRefundError);
                    }
                }

                _orgUnitOfWork.CommitChanges();
            }
            catch
            {
                _orgUnitOfWork.SafeRollback();
                throw;
            }

            // 自动发货发通知
            if (cmd.IsFromAuto)
            {
                var orderDetailCtn = (order.Prods?.FirstOrDefault() as CourseOrderProdItemDto);

                // 微信
                {
                    try
                    {
                        await _mediator.Send(new SendWxTemplateMsgCmd
                        {
                            UserId = order.UserId,
                            WechatTemplateSendCmd = new WechatTemplateSendCmd()
                            {
                                KeyWord1 = $"您购买的《{orderDetailCtn.Title}》已完成。",
                                KeyWord2 = DateTime.Now.ToDateTimeString(),
                                Remark = "点击查看订单详情",
                                MsyType = WechatMessageType.订单已完成,
                                OrderID = order.OrderId,
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        //throw new CustomResponseException("自动确定收货-发送微信通知失败: " + ex.Message, Consts.Err.AutoShippedOrder_send_wx_Error);
                    }
                }
                // 短信不管
                {
                }
            }

            // 确认收货导致取消wx通知
            for (var __ = rdfs?.Count > 0; __; __ = !__)
            {
                var rdf = rdfs[0];
                var cprod = order.Prods.FirstOrDefault(_ => _.OrderDetailId == rdf.OrderDetailId) as CourseOrderProdItemDto;
                if (cprod == null) continue;
                try
                {
                    await _mediator.Send(new SendWxTemplateMsgCmd
                    {
                        UserId = order.UserId,
                        WechatTemplateSendCmd = new WechatTemplateSendCmd
                        {
                            KeyWord1 = $"您发起的《{cprod.Title}》{rdf.Count}件商品已确认收货，{(rdf.Type == (int)RefundTypeEnum.Refund ? "退款" : "退货退款")}申请已取消！",
                            KeyWord2 = DateTime.Now.ToDateTimeString(),
                            Remark = "点击下方【详情】查看退款详情",
                            MsyType = WechatMessageType.确认收货导致退款申请取消,
                        }
                    });
                }
                catch { }
            }

            // 成功后续
            AsyncUtils.StartNew(new OrderShippedOkEvent { OrderId = cmd.OrderId, IsFromAuto = cmd.IsFromAuto });

            return true;
        }

    }

    public class OrderShippedOkEventHandler : INotificationHandler<OrderShippedOkEvent>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        private readonly CSRedisClient redis;
        private readonly IConfiguration _config;
        private readonly IServiceProvider services;
        IPointsMallService _pointsMallService;
        IDelayTaskService _delayTaskService;
        ILogger<OrderShippedOkEventHandler> _logger;

        public OrderShippedOkEventHandler(IOrgUnitOfWork orgUnitOfWork
            , CSRedisClient redis
            , IConfiguration _config
            , IMediator mediator
            , IServiceProvider services
            , IPointsMallService pointsMallService
            , IDelayTaskService delayTaskService
            , ILogger<OrderShippedOkEventHandler> logger)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this.redis = redis;
            this._config = _config;
            this.services = services;
            _pointsMallService = pointsMallService;
            _delayTaskService = delayTaskService;
            _logger = logger;
        }

        public async Task Handle(OrderShippedOkEvent e, CancellationToken cancellationToken)
        {
            var orders = await _mediator.Send(new OrderDetailSimQuery { OrderId = e.OrderId });
            var order = orders.Orders?.FirstOrDefault();
            if (order == null)
                throw new CustomResponseException("无效的订单", Consts.Err.OrderIsNotValid_OnShipped);

            if (((OrderStatusV2)order.OrderStatus) != OrderStatusV2.Shipped)
                throw new CustomResponseException("当前订单状态不能确定收货", Consts.Err.OrderStatus_IsNot_Shipping);

            var ver = order.Prods?.FirstOrDefault()?._Ver;
            var payedTime = order.UserPayTime ?? DateTime.Now.AddSeconds(-2);

            // add种草获奖机会
            if (!ver.IsNullOrEmpty())
            {
                var startTime = DateTime.Parse(_config["AppSettings:EvltReward:StartTime"]);
                var endTime = DateTime.Parse(_config["AppSettings:EvltReward:EndTime"]);
                do
                {
                    if (payedTime < startTime || payedTime > endTime) break;
                    // 后续处理 商品单价超过x元才算
                    Debugger.Break();
                    // 当成活动
                    // 目前网课是只能1个订单并且订单里只能1个
                    //
                    var user_IsFxAdviser = false;
                    var _ParentUserId = (Guid?)null;
                    if (order.Prods.Where(_ => _.ProdType == CourseTypeEnum.Course.ToInt()).Count() > 0)
                    {
                        // 网课
                        var rr = await _mediator.Send(new CheckIsFxHeadQuery { UserId = order.UserId });
                        user_IsFxAdviser = rr.IsHead;
                        _ParentUserId = rr.HeadFxUserId == default ? (Guid?)null : rr.HeadFxUserId;
                    }
                    await _mediator.Send(new PresetEvltRewardChangesCmd
                    {
                        OrderDetail = order,
                        FxHeadUserId = _ParentUserId,
                        IsFxAdviser = user_IsFxAdviser,
                    });
                } while (false);
            }

            // 确认收货确定佣金有效
            {
                (object r, Exception ex) = (null, null);
                try { r = await _mediator.Send(new OrderShippedOkThenSettleCmd { Order = order }); }
                catch (Exception ex0) { ex = ex0; }
                if (ex != null)
                {
                    services.GetService<NLog.ILogger>().Error(services.GetNLogMsg("确认收货确定佣金有效").SetTime(DateTime.Now)
                        .SetLevel("Error").SetUserId(order.UserId)
                        .SetParams(e)
                        .SetClass(nameof(OrderShippedOkEventHandler))
                        .SetError(ex));
                }
                else if (r == null)
                {
                    services.GetService<NLog.ILogger>().Warn(services.GetNLogMsg("确认收货确定佣金有效").SetTime(DateTime.Now)
                        .SetLevel("Warn").SetUserId(order.UserId)
                        .SetParams(e)
                        .SetClass(nameof(OrderShippedOkEventHandler))
                        .SetContent("找不到CourseDrpInfo配置"));
                }
            }

            // 解冻好物新人等奖励
            {
                foreach (var courseOrderProdItemDto in order.Prods.OfType<CourseOrderProdItemDto>())
                {
                    if (!(courseOrderProdItemDto._ctn?["_FreezeMoneyInLogIds"] is JObject freezeMoneyInLogIdDto)) continue;
                    await _mediator.Send(new WalletInsideUnFreezeAmountApiArgs
                    {
                        FreezeMoneyInLogId = freezeMoneyInLogIdDto["id"].ToString(),
                        Type = (int)freezeMoneyInLogIdDto["type"],
                        _others = new { courseOrderProdItemDto.OrderDetailId },
                    });
                }
            }

            // 成功给王宁dalao打卡...
            try
            {
                await services.GetService<IntegrationEvents.IOrganizationIntegrationEventService>().PublishEventAsync(new IntegrationEvents.Events.OrderShippedOkIntegrationEvent
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId
                });
            }
            catch { }


            //赠送冻结积分
            foreach (var courseOrderProdItemDto in order.Prods.OfType<CourseOrderProdItemDto>())
            {
                try
                {
                    var freezeId = await redis.GetAsync<Guid>(CacheKeys.PresentedFreezePointsCacheKey(courseOrderProdItemDto.OrderDetailId));
                    if (freezeId != default(Guid))
                    {
                        var courseId = courseOrderProdItemDto.Id;
                        var courseDrpinfo = await _mediator.Send(new GetCourseFxSimpleInfoQuery { CourseId = courseId });
                        var receivingAfterDays = courseDrpinfo.ReceivingAfterDays ?? 3;
                        int delay = TimeSpan.FromDays(receivingAfterDays).Milliseconds;
                        var comfirmReceiveDeFreezePointsCommand = new ComfirmReceiveDeFreezePointsCommand()
                        {
                            FreezeId = freezeId,
                            UserId = order.UserId
                        };
                        _delayTaskService.Set(comfirmReceiveDeFreezePointsCommand, delay);
                        await redis.DelAsync(CacheKeys.PresentedFreezePointsCacheKey(courseOrderProdItemDto.OrderDetailId));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"赠送冻结积分失败，orderDetailId = {courseOrderProdItemDto.OrderDetailId}");
                }
            }


        }
    }
}
