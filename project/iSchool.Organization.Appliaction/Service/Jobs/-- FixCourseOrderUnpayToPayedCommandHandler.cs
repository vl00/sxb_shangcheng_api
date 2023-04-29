using AutoMapper;
using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
     // old code
//    public class FixCourseOrderUnpayToPayedCommandHandler : IRequestHandler<FixCourseOrderUnpayToPayedCommand>
//    {
//        private readonly OrgUnitOfWork _orgUnitOfWork;
//        private readonly IMediator _mediator;        
//        private readonly CSRedisClient redis;                
//        private readonly IConfiguration _config;
//        private readonly NLog.ILogger log;
//        private readonly ILock1Factory _lck1fay;

//        public FixCourseOrderUnpayToPayedCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
//            IConfiguration config, ILock1Factory lck1fay,
//            IServiceProvider services)
//        {
//            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
//            this._mediator = mediator;
//            this.redis = redis;            
//            this._config = config;
//            _lck1fay = lck1fay;
//            this.log = services.GetService<NLog.ILogger>();
//        }

//        public async Task<Unit> Handle(FixCourseOrderUnpayToPayedCommand cmd, CancellationToken cancellation)
//        {
//            while (!cancellation.IsCancellationRequested)
//            {
//                Debugger.Break();
//                var orders = await GetOrders();
//                if (orders?.Length < 1) break;

//                foreach (var order in orders)
//                {
//                    var fcr_status = FinanceCenterOrderStatus.Wait.ToInt();
//                    try
//                    {
//                        var fcr = await _mediator.Send(new FinanceCheckOrderPayStatusQuery { OrderId = order.Id });
//                        fcr_status = fcr.OrderStatus;
//                    }
//                    catch (Exception ex)
//                    {
//                        continue;
//                    }

//                    var oldOrderStatus = order.Status;
//                    var newOrderStatus = (FinanceCenterOrderStatus)fcr_status switch
//                    {
//                        FinanceCenterOrderStatus.Wait => (int)OrderStatusV2.Unpaid,
//                        FinanceCenterOrderStatus.Process => (int)OrderStatusV2.Paiding,
//                        FinanceCenterOrderStatus.PaySucess => (int)OrderStatusV2.Paid,
//                        FinanceCenterOrderStatus.PayFaile => (int)OrderStatusV2.PaidFailed,
//                        FinanceCenterOrderStatus.Cancel => (int)OrderStatusV2.PaidFailed,
//                        _ => (int)OrderStatusV2.Completed,
//                    };
//                    if (newOrderStatus != (int)OrderStatusV2.Paid) continue;
//                    if (oldOrderStatus == (int)OrderStatusV2.Paid) continue;

//                    // fix order status
//                    await _mediator.Send(new UpdateOrderStatusCommand
//                    {
//                        OrderId = order.Id,
//                        NewStatus = newOrderStatus,
//                        NewStatusOk_Paymenttime = order.CreateTime?.AddSeconds(5),
//                    });

//                    var (orderDetail, prod) = await GetOrderDetial(order);
//                    var ctn = string.IsNullOrEmpty(orderDetail.Ctn) ? null : JObject.Parse(orderDetail.Ctn);

//                    // fix stock
//                    var sellcount = prod?.BuyAmount ?? 0;                    
//                    if (sellcount != 0 && oldOrderStatus == (int)OrderStatusV2.Cancelled)
//                    {
//                        var sr = (await _mediator.Send(new CourseGoodsStockRequest
//                        {
//                            StockCmd = new GoodsStockCommand { Id = prod.GoodsId, Num = 0 }
//                        })).StockResult;
//                        var sr1 = (await _mediator.Send(new CourseGoodsStockRequest
//                        {
//                            StockCmd = new GoodsStockCommand { Id = prod.GoodsId, Num = sellcount }
//                        })).StockResult;
//                    }

//                    // reset poll cache
//                    {
//                        var args_poll = new PollCallRequest
//                        {
//                            SetResultCmd = new PollSetResultCommand
//                            {
//                                Id = CacheKeys.OrderPoll_wxpay_order.FormatWith(order.Id.ToString("n")),
//                                CheckIfExists = false,
//                                Result = new WxPayOkOrderDto
//                                {
//                                    OrderId = order.Id,
//                                    OrderNo = order.Code,
//                                    PayIsOk = true,
//                                    UserPayTime = order.CreateTime?.AddSeconds(22),
//                                    Paymoney = order.Totalpayment ?? 0,
//                                    OrderType = order.Type ?? 0,
//                                    BuyAmount = sellcount,
//                                    CourseId = prod.Id,
//                                    GoodsId = prod.GoodsId,
//                                    FxHeaducode = (string)ctn["_FxHeaducode"],
//                                    _Ver = (string)ctn["_Ver"],
//                                },
//                                ExpSec = 60 * 30,
//                            }
//                        };
//                        await _mediator.Send(args_poll);
//                    }

//                    await _mediator.Publish(new OrderPayedOkEvent { OrderId = order.Id });
//                }

//                await DelTmp(orders.Select(_ => _.Id));
//            }
//            return default;
//        }

//        private async Task<Order[]> GetOrders()
//        {
//            var sql = $@"
//select top 5 o.* from [Order] o where exists(select 1 from [tmp_fix_order] where id=o.id 
//--and status in({OrderStatusV2.Cancelled.ToInt()}, {OrderStatusV2.PaidFailed.ToInt()})
//) order by o.CreateTime
//";
//            // must use write connection
//            var ls = await _orgUnitOfWork.DbConnection.QueryAsync<Order>(sql); 
//            return ls.AsArray();
//        }

//        private async Task<(OrderDetial, CourseOrderProdItemDto)> GetOrderDetial(Order order)
//        {
//            var sql = $@"
//select top 1 * from [OrderDetial] p where p.orderid=@orderId
//";
//            var d = await _orgUnitOfWork.QueryFirstOrDefaultAsync<OrderDetial>(sql, new { orderId = order.Id });

//            var prods = (await _mediator.Send(new OrderProdsByOrderIdsQuery
//            {
//                Orders = new[] { (order.Id, ((OrderType)(order.Type ?? 0))) }
//            })).OrderProducts
//            .FirstOrDefault().Products ?? new OrderProdItemDto[0];

//            return (d, prods.FirstOrDefault() as CourseOrderProdItemDto);
//        }

//        private async Task DelTmp(IEnumerable<Guid> orderIds)
//        {
//            var sql = "delete from [tmp_fix_order] where id in @orderIds";
//            await _orgUnitOfWork.ExecuteAsync(sql, new { orderIds });
//        }

//        NLog.LogEventInfo GetLogMsg(object paramsObj = null)
//        {
//            var msg = new NLog.LogEventInfo();
//            msg.Properties["Time"] = DateTime.Now.ToMillisecondString();
//            msg.Properties["Caption"] = "修正课程订单";
//            //msg.Properties["UserId"] = me.UserId;
//            msg.Properties["Level"] = "Error";
//            if (paramsObj is string str) msg.Properties["Params"] = str;
//            else if (paramsObj != null) msg.Properties["Params"] = (paramsObj).ToJsonString(camelCase: true);
//            //msg.Properties["Error"] = $"检测敏感词意外失败.网络异常.err={ex.Message}";
//            //msg.Properties["StackTrace"] = ex.StackTrace;
//            //msg.Properties["ErrorCode"] = 3;
//            return msg;
//        }
//    }
}
