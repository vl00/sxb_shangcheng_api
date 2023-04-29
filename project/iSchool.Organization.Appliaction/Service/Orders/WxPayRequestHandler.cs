using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public partial class WxPayRequestHandler : IRequestHandler<WxPayRequest, WxPayResponse>
    {
        IMediator _mediator;
        IServiceProvider services;
        IConfiguration config;

        public WxPayRequestHandler(IMediator mediator,
            IConfiguration config,
            IServiceProvider services)
        {
            this._mediator = mediator;
            this.services = services;
            this.config = config;
        }

        public async Task<WxPayResponse> Handle(WxPayRequest request, CancellationToken cancellation)
        {
            var response = new WxPayResponse();
            if (request.AddPayOrderRequest != null) response.AddPayOrderResponse = await OnHandle(request.AddPayOrderRequest);
            if (request.WxPayCallback != null) await OnHandle_callback(request.WxPayCallback);
            return response;
        }

        /// <summary>请求wx预支付</summary>
        internal async Task<JToken> OnHandle(ApiWxAddPayOrderRequest query)
        {
            var httpClientFactory = services.GetService<IHttpClientFactory>();
            var log = services.GetService<NLog.ILogger>();
            var addpayUrl = config["AppSettings:wxpay:addpayUrl"];
            var paykey = config["AppSettings:wxpay:paykey"];
            var system = config["AppSettings:wxpay:system"];
            await default(ValueTask);
            
            using var http = httpClientFactory.CreateClient(string.Empty);
            var body = query.ToJsonString(true);
            var rr = await new HttpApiInvocation(log)
                .SetApiDesc("请求wx预支付")
                .SetMethod(HttpMethod.Post).SetUrl(addpayUrl)
                .SetBodyByJsonStr(body)
                .OnBeforeRequest(req =>
                {
                    req.SetFinanceSignHeader(paykey, body, system);
                })
                .InvokeByAsync<JToken>(http);

            if (!rr.Succeed)
            {
                throw new CustomResponseException(rr.Msg, (int)rr.status);
            }

            return rr.Data;
        }      
    }

    public partial class WxPayRequestHandler
    {
        /// <summary>wx支付回调</summary>
        internal async Task OnHandle_callback(WxPayCallbackNotifyMessage e)
        {
            var redis = services.GetService<CSRedisClient>();
            var orgUnitOfWork = services.GetService<IOrgUnitOfWork>() as OrgUnitOfWork;
            var log = services.GetService<NLog.ILogger>();
            var _lckfay1 = services.GetService<ILock1Factory>();
            await default(ValueTask);

            if (e.PayStatus == WxPayCallbackNotifyPayStatus.InProcess)
            {
                throw new CustomResponseException("支付状态错误", 500);
            }

            var k = CacheKeys.OrderPoll_wxpay_order.FormatWith(e.OrderId.ToString("n"));
            var pr = (await _mediator.Send(new PollCallRequest
            {
                Query = new PollQuery { Id = k, IgnoreRrc = true, DelayMs = -1 }
            })).PollQryResult;

            if (!(pr.Result is WxPayOkOrderDto rr))
            {
                throw new CustomResponseException($"不是机构平台的单 或 订单轮询cache没该key={k}");
            }
            if (rr.PayIsOk != null)
            {
                // 重复called
                return;
            }
            Debugger.Break();

            var pfky = $"order_{e.OrderId:n}_payis_{e.PayStatus.GetName().ToLower()}";
            var pfkyid = Guid.NewGuid().ToString();
            if (!(await redis.SetAsync(CacheKeys.PayNorepear.FormatWith(pfky), pfkyid, 120, RedisExistence.Nx)))
            {
                // 重复called
                log.Info(GetPayedLogMsg("warn")
                    .SetContent("支付成功回调检查到是重复处理")
                    .SetClass(nameof(WxPayRequestHandler)).SetMethod(nameof(OnHandle_callback))
                    .SetParams(new { e })
                    .SetTime(DateTime.Now));
                return;
            }

            // write db
            var (upStatusIsOK, status0, modifier0) = (false, 0, default(Guid?));
            for (var ___ = true; ___; ___ = !___)
            {
                // try update order status
                if (e.PayStatus == WxPayCallbackNotifyPayStatus.Success)
                {
                    try
                    {
                        (upStatusIsOK, status0, modifier0) = await UpOrderStatusToPaided(
                            e.OrderId, e.AddTime, rr._Modifier, rr.Prods?.Select(_ => _.OrderId),
                            _lckfay1, orgUnitOfWork);
                    }
                    catch (Exception ex)
                    {
                        log.Error(GetPayedLogMsg("error")
                            .SetContent("支付成功回调更新订单状态失败")
                            .SetClass(nameof(WxPayRequestHandler)).SetMethod(nameof(OnHandle_callback))
                            .SetParams(new { e, status0, modifier0, AdvanceOrderNo = rr.OrderNo })
                            .SetError(ex, 500)
                            .SetTime(DateTime.Now));

                        _ = redis.LockExReleaseAsync(CacheKeys.PayNorepear.FormatWith(pfky), pfkyid);

                        throw ex;
                    }
                }

                if (e.PayStatus == WxPayCallbackNotifyPayStatus.InProcess) break;
                for (var __ = e.PayStatus == WxPayCallbackNotifyPayStatus.Success && !upStatusIsOK; __; __ = !__)
                {
                    if (status0 == -1)
                    {
                        log.Info(GetPayedLogMsg("warn")
                            .SetContent("支付成功回调更新订单状态失败")
                            .SetClass(nameof(WxPayRequestHandler)).SetMethod(nameof(OnHandle_callback))
                            .SetParams(new { e, status0, modifier0, AdvanceOrderNo = rr.OrderNo })
                            .SetTime(DateTime.Now));

                        throw new CustomResponseException($"支付成功回调更新订单状态失败.预订单id={e.OrderId}");
                    }
                    if (!(status0 == (int)OrderStatusV2.Cancelled && modifier0 != Guid.Empty))
                    {
                        // 重复called
                        return;
                    }
                    //
                    // 支付成功后回调前, 发现订单(过期)已关闭
                    // 此时进行退款
                    //
                    log.Info(GetPayedLogMsg("debug")
                        .SetContent("支付成功回调发现订单已关闭而进行退款")
                        .SetClass(nameof(WxPayRequestHandler)).SetMethod(nameof(OnHandle_callback))
                        .SetParams(new { e, status0, modifier0, AdvanceOrderNo = rr.OrderNo })
                        .SetTime(DateTime.Now));

                    var rfr = await _mediator.Send(new RefundCmd
                    {
                        AdvanceOrderId = e.OrderId,
                        RefundAmount = rr.Paymoney!.Value,
                        Remark = "支付成功回调发现订单已关闭而进行退款",
                        RefundType = 1,
                        _others = new { status0, modifier0, AdvanceOrderNo = rr.OrderNo }
                    });

                    if (rfr.ApplySucess)
                    {
                        try
                        {
                            await _mediator.Send(new SendWxTemplateMsgCmd
                            {
                                UserId = rr.UserId,
                                WechatTemplateSendCmd = new WechatTemplateSendCmd
                                {
                                    KeyWord1 = $"您支付的订单已超时，支付款项已原路退回。",
                                    KeyWord2 = DateTime.Now.ToDateTimeString(),
                                    Remark = "点击更多查看详情",
                                    MsyType = WechatMessageType.支付成功回调发现订单已关闭而进行退款,
                                }
                            });
                        }
                        catch { }
                    }
                }

                // 支付成功|失败 同步库存并回到db
                try
                {
                    if (rr.OrderType == (int)OrderType.BuyCourseByWx && upStatusIsOK)
                    {
                        await SyncSkuStockAfterPayed(rr, e.PayStatus);
                    }
                }
                catch (Exception ex)
                {
                    var msg = GetPayedLogMsg("error");
                    msg.Properties["Error"] = $"[订单{e.OrderId}]支付回调后同步库存失败.err={ex.Message}";
                    msg.Properties["StackTrace"] = ex.StackTrace;
                    msg.Properties["ErrorCode"] = 500;
                    log.Error(msg);
                    throw ex;
                }
            }

            if (e.PayStatus == WxPayCallbackNotifyPayStatus.Success && upStatusIsOK)
            {
                rr.PayIsOk = true;
                rr.UserPayTime = e.AddTime;
            }
            else
            {
                rr.PayIsOk = false;
                rr.Paymoney = null;
                rr.UserPayTime = null;
            }

            // Set result(ok) to front-side
            try
            {
                await _mediator.Send(new PollCallRequest
                {
                    SetResultCmd = new PollSetResultCommand
                    {
                        Id = k,
                        Result = rr,
                        CheckIfExists = true,
                        Rrc = -1,
                    }
                });
            }
            catch (Exception ex)
            {
                var msg = GetPayedLogMsg("error");
                msg.Properties["Error"] = $"[订单{e.OrderId}]支付回调后set poll result失败.err={ex.Message}";
                msg.Properties["StackTrace"] = ex.StackTrace;
                msg.Properties["ErrorCode"] = 500;
                log.Error(msg);
                throw ex;
            }

            // After payed ok
            if (e.PayStatus == WxPayCallbackNotifyPayStatus.Success && upStatusIsOK)
            {
                AsyncUtils.StartNew(new OrderPayedOkEvent { OrderId = e.OrderId });
            }
        }

        protected async Task SyncSkuStockAfterPayed(WxPayOkOrderDto rr, WxPayCallbackNotifyPayStatus e_PayStatus)
        {
            foreach (var sku in rr.Prods)
            {
                await _mediator.Send(new CourseGoodsStockRequest
                {
                    SyncSetStock = new SyncSetGoodsStockCommand
                    {
                        Id = sku.GoodsId,
                        AddNum = (e_PayStatus == WxPayCallbackNotifyPayStatus.Success ? 0 : sku.BuyCount)
                    }
                });
            }
        }

        protected async Task<(bool IsOK, int Status0, Guid? Modifier0)> UpOrderStatusToPaided(Guid advanceOrderId, DateTime paymenttime, Guid? modifier, IEnumerable<Guid> orderIds,
            ILock1Factory _lckfay1, OrgUnitOfWork _orgUnitOfWork)
        {
            if (orderIds == null || !orderIds.Any())
            {
                orderIds = await _orgUnitOfWork.QueryAsync<Guid>("select id from [Order] where [AdvanceOrderId]=@advanceOrderId", new { advanceOrderId });
                if (orderIds == null || !orderIds.Any())
                    orderIds = await _orgUnitOfWork.DbConnection.QueryAsync<Guid>("select id from [Order] where [AdvanceOrderId]=@advanceOrderId", new { advanceOrderId });
            }

            await using var _lck_ = await _lckfay1.LockAsync($"org:lck2:order_{advanceOrderId:n}_up2paied", retry: 1);
            if (!_lck_.IsAvailable) return (false, -1, default);
            try
            {
                _orgUnitOfWork.BeginTransaction();

                var sql = $@"
declare @i int
declare @status00 int
declare @Modifier0 uniqueidentifier

update [Order] set [status]=@NewStatus,[Paymenttime]=@Paymenttime,[ModifyDateTime]=sysdatetime(),[Modifier]='{(modifier == null ? "11111111-1111-1111-1111-111111111111" : modifier.ToString())}'
    where [status]={OrderStatusV2.Unpaid.ToInt()} and [AdvanceOrderId]=@AdvanceOrderId 

set @i=@@ROWCOUNT
if @i>0 begin
    update [OrderDetial] set [status]=@NewStatus where [status]={OrderStatusV2.Unpaid.ToInt()} and [orderid] in @orderIds
end 
else begin
    select top 1 @Modifier0=[Modifier],@status00=[status] from [Order] where [AdvanceOrderId]=@AdvanceOrderId

    update [Order] set [Modifier]='00000000-0000-0000-0000-0000000000000' where [AdvanceOrderId]=@AdvanceOrderId
end

select @i,@status00,@Modifier0
";
                var (i, status0, modifier0) = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<(int, int, Guid?)>(sql, new
                {
                    AdvanceOrderId = advanceOrderId,
                    orderIds,
                    NewStatus = (int)OrderStatusV2.Paid,
                    Paymenttime = paymenttime,
                    now = paymenttime < DateTime.Now ? DateTime.Now : paymenttime.AddSeconds(1),
                }, _orgUnitOfWork.DbTransaction);

                var b = i > 0;
                _orgUnitOfWork.CommitChanges();

                return (b, b ? (int)OrderStatusV2.Unpaid : status0, b ? null : modifier0);
            }
            catch
            {
                _orgUnitOfWork.SafeRollback();
                throw;
            }
        }

        NLog.LogEventInfo GetPayedLogMsg(string level = null)
        {
            var msg = new NLog.LogEventInfo();
            msg.SetClass(nameof(WxPayRequestHandler));
            msg.Properties["Time"] = DateTime.Now.ToMillisecondString();
            msg.Properties["Content"] = "wx购买课程后支付回调";
            //msg.Properties["UserId"] = me.UserId;
            msg.Properties["Level"] = level; 
            //msg.Properties["Error"] = $"XXXXX.err={ex.Message}";
            //msg.Properties["StackTrace"] = ex.StackTrace;
            //msg.Properties["ErrorCode"] = 3;
            return msg;
        }
    }
}
