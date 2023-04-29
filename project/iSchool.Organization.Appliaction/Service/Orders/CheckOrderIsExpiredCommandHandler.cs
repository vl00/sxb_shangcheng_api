using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class CheckOrderIsExpiredCommandHandler : IRequestHandler<CheckOrderIsExpiredCommand, object>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;
        ILock1Factory _lck1fay;
        NLog.ILogger log;

        public CheckOrderIsExpiredCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            ILock1Factory lck1fay, NLog.ILogger log,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
            this._lck1fay = lck1fay;
            this.log = log;
        }

        public async Task<object> Handle(CheckOrderIsExpiredCommand cmd, CancellationToken cancellation)
        {
            var k = cmd.AdvanceOrderId == null ? "org:lck2:check_orders_expired" : $"org:lck2:check_orders_expired:aoid_{cmd.AdvanceOrderId}";
            await using var lck0 = await _lck1fay.LockAsync(k, 1000 * 60 * 3, 1);
            if (!lck0.IsAvailable) return false;

            var sql = $@"
select distinct top 100 o.AdvanceOrderId,AdvanceOrderNo,o.status,o.createtime 
from [order] o with(nolock)
where o.type>={OrderType.BuyCourseByWx.ToInt()} and o.IsValid=1 and o.status={OrderStatusV2.Unpaid.ToInt()}
{$"and datediff(second,o.CreateTime,getdate())>={60 * 30}".If(cmd.AdvanceOrderId == null)}
{"and o.AdvanceOrderId=@AdvanceOrderId".If(cmd.AdvanceOrderId != null)}
order by o.CreateTime desc
";
            var orders = (await _orgUnitOfWork.DbConnection.QueryAsync<SimOrder>(sql, new { cmd.AdvanceOrderId })).AsList();
            if (orders.Count < 1) return false;

            var ordersLs = await FindOrders(orders.Select(_ => _.AdvanceOrderId));
            if (ordersLs.Count < 1) return false;

            // 查一次支付中心 + re回调实际已支付的单
            var torm = new List<Guid>();
            foreach (var order in orders)
            {
                var oldOrderStatus = order.Status;
                if (oldOrderStatus != (int)OrderStatusV2.Unpaid) continue;

                // re回调实际已支付的单
                var b = await _mediator.Send(new TryRePayorderCommand { OrdersEntity = ordersLs.Find(_ => _.AdvanceOrderId == order.AdvanceOrderId) });
                if (b)
                {
                    torm.Add(order.AdvanceOrderId);
                }
            }
            if (torm.Count > 0)
            {
                orders.RemoveAll(_ => torm.Contains(_.AdvanceOrderId));
                ordersLs.RemoveAll(_ => torm.Contains(_.AdvanceOrderId));
            }
            if (ordersLs.Count < 1) return false;
            if (orders.Count < 1) return false;

            // try cancel
            foreach (var order in orders)
            {
                if ((DateTime.Now - order.CreateTime) < TimeSpan.FromMinutes(30))
                {
                    if (cmd.AdvanceOrderId == null) continue;
                    else return false;
                }
                try
                {
                    await _mediator.Send(new MiniOrderCancelCmd 
                    { 
                        OrderId = order.AdvanceOrderId,
                        IsFromExpired = true,
                    });
                }
                catch { }
            }

            return true;
        }

        private async Task<List<OrderDetailSimQryResult>> FindOrders(IEnumerable<Guid> advanceOrderIds)
        {
            var ls = new List<OrderDetailSimQryResult>();
            foreach (var aoid in advanceOrderIds)
            {
                var o = await _mediator.Send(new OrderDetailSimQuery { AdvanceOrderId = aoid, IgnoreCheckExpired = true });
                ls.Add(o);
            }
            return ls;
        }

        class SimOrder
        {
            public Guid AdvanceOrderId { get; set; }
            public string AdvanceOrderNo { get; set; }
            public int Status { get; set; }
            public DateTime CreateTime { get; set; }
        }

        NLog.LogEventInfo GetLogMsg(object paramsObj = null)
        {
            var msg = new NLog.LogEventInfo();
            msg.Properties["Time"] = DateTime.Now.ToMillisecondString();
            msg.Properties["Caption"] = "订单过期";
            //msg.Properties["UserId"] = 
            msg.Properties["Level"] = "Error";
            if (paramsObj is string str) msg.Properties["Params"] = str;
            else if (paramsObj != null) msg.Properties["Params"] = (paramsObj).ToJsonString(camelCase: true);
            //msg.Properties["Error"] = $"检测敏感词意外失败.网络异常.err={ex.Message}";
            //msg.Properties["StackTrace"] = ex.StackTrace;
            //msg.Properties["ErrorCode"] = 3;
            return msg;
        }
    }
}
