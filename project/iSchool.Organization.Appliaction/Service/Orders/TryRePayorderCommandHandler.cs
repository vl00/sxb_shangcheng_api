using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class TryRePayorderCommandHandler : IRequestHandler<TryRePayorderCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;                
        IConfiguration _config;

        public TryRePayorderCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;            
            this._config = config;
        }

        public async Task<bool> Handle(TryRePayorderCommand cmd, CancellationToken cancellation)
        {
            var orders = cmd.OrdersEntity;
            if (orders == null)
            {
                if (cmd.AdvanceOrderId == null) return false;

                orders = await _mediator.Send(new OrderDetailSimQuery { AdvanceOrderId = cmd.AdvanceOrderId.Value, IgnoreCheckExpired = true, UseReadConn = false });
            }
            if (orders.Orders.Any(_ => _.OrderStatus >= (int)OrderStatusV2.Paid))
            {
                return false;
            }

            // 查一次支付中心
            var fcr = default(FinanceCheckOrderPayStatusQryResult);
            try{ fcr = await _mediator.Send(new FinanceCheckOrderPayStatusQuery { OrderId = orders.AdvanceOrderId }); }
            catch { }
            var newOrderStatus = (FinanceCenterOrderStatus?)fcr?.OrderStatus switch
            {
                FinanceCenterOrderStatus.Wait => (int)OrderStatusV2.Unpaid,
                FinanceCenterOrderStatus.Process => (int)OrderStatusV2.Paiding,
                FinanceCenterOrderStatus.PaySucess => (int)OrderStatusV2.Paid,
                FinanceCenterOrderStatus.PayFaile => (int)OrderStatusV2.PaidFailed,
                FinanceCenterOrderStatus.Cancel => (int)OrderStatusV2.PaidFailed,
                _ => -1,
            };
            var userpaytime = fcr?.PaySuccessTime != default && fcr.PaySuccessTime != default(DateTime) ? fcr.PaySuccessTime.Value : orders.Orders[0].OrderCreateTime.AddSeconds(90);
            //
            // re回调实际已支付的单
            if (newOrderStatus == (int)OrderStatusV2.Paid)
            {
                var ctn = (orders.Orders[0].Prods?.FirstOrDefault() as CourseOrderProdItemDto)?._ctn;
                var prods = orders.Orders.SelectMany(x => x.Prods.OfType<CourseOrderProdItemDto>().Select(_ => (x.OrderId, Prod: _))).ToArray();
                // reset poll cache
                var args_poll = new PollCallRequest
                {
                    SetResultCmd = new PollSetResultCommand
                    {
                        Id = CacheKeys.OrderPoll_wxpay_order.FormatWith(orders.AdvanceOrderId.ToString("n")),
                        CheckIfExists = false,
                        Result = new WxPayOkOrderDto
                        {
                            OrderId = orders.AdvanceOrderId,
                            OrderNo = orders.AdvanceOrderNo,
                            AdvanceOrderId = orders.Orders.Length > 1 ? orders.AdvanceOrderId : (Guid?)null,
                            AdvanceOrderNo = orders.AdvanceOrderNo,
                            PayIsOk = null, // ignore
                            UserPayTime = userpaytime,
                            UserId = orders.UserId,
                            Paymoney = null, // ignore
                            OrderType = (int)orders.OrderType,

                            //BuyAmount = prods.Length == 1 ? prods[0].BuyCount : default,
                            //CourseId = prods.Length == 1 ? prods[0].Id : default,
                            //GoodsId = prods.Length == 1 ? prods[0].GoodsId : default,
                            //Prods = prods.Length == 1 ? null : prods.Select(g => (g.GoodsId, g.Id, g.BuyCount)).ToArray(),
                            Prods = prods.Select(c => (c.Prod.OrderDetailId, c.OrderId, c.Prod.GoodsId, c.Prod.Id, c.Prod.BuyCount)).ToArray(),

                            FxHeaducode = (string)ctn?["_FxHeaducode"],
                            _Ver = (string)ctn?["_Ver"],
                            _Modifier = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                        },
                        ExpSec = 60 * 60 * 3,
                    }
                };
                await _mediator.Send(args_poll);
                // call back
                await _mediator.Send(new WxPayRequest
                {
                    WxPayCallback = new ViewModels.WxPayCallbackNotifyMessage
                    {
                        OrderId = orders.AdvanceOrderId,
                        PayStatus = WxPayCallbackNotifyPayStatus.Success,
                        AddTime = userpaytime,
                    }
                });

                return true;
            }

            return false;
        }

    }
}
