using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    public class OrderShippedAutoCmdHandler : IRequestHandler<OrderShippedAutoCmd, OrderShippedAutoCmdResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;                
        IConfiguration _config;
        ILock1Factory _lck1fay;
        IServiceProvider services;

        public OrderShippedAutoCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            ILock1Factory lck1fay,
            IConfiguration config, IServiceProvider services)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;            
            this._config = config;
            this._lck1fay = lck1fay;
            this.services = services;
        }

        public async Task<OrderShippedAutoCmdResult> Handle(OrderShippedAutoCmd cmd, CancellationToken cancellation)
        {
            var result = new OrderShippedAutoCmdResult();

            var sql = $@"--SendExpressTime
select top 100 o.* from [order] o 
where o.type>={OrderType.BuyCourseByWx.ToInt()} and o.IsValid=1 and o.status={OrderStatusV2.Shipping.ToInt()} 
and o.ShippingTime is not null and datediff(hour,o.ShippingTime,getdate())>=24*{cmd.Days}
and not exists(select 1 from [OrderRefunds] r where r.IsValid=1 and r.orderid=o.id and not(r.type in @tys or r.Status in @stt) )
order by o.ShippingTime
";
            var orders = (await _orgUnitOfWork.QueryAsync<Order>(sql, new 
            {
                tys = (new[] { RefundTypeEnum.FastRefund, RefundTypeEnum.BgRefund }).Select(_ => (int)_),
                stt = (new[]
                {
                    RefundStatusEnum.RefundSuccess, RefundStatusEnum.ReturnSuccess,
                    RefundStatusEnum.RefundAuditFailed, RefundStatusEnum.ReturnAuditFailed, RefundStatusEnum.InspectionFailed,
                    RefundStatusEnum.Cancel, RefundStatusEnum.CancelByExpired
                }).Select(_ => (int)_),
            })).AsList();
            if (orders.Count < 1) return result;

            List<Guid> notOkOrderIds = null;
            List<Exception> errs = null;
            foreach (var order in orders)
            {
                try
                {
                    await _mediator.Send(new MiniOrderShippedCmd { OrderId = order.Id, IsFromAuto = true });
                }
                catch (Exception ex)
                {
                    notOkOrderIds ??= new List<Guid>();
                    notOkOrderIds.Add(order.Id);
                    errs ??= new List<Exception>();
                    errs.Add(ex);

                    services.GetService<NLog.ILogger>().Error(services.GetNLogMsg("自动确认收货失败").SetTime(DateTime.Now)
                        .SetLevel("Error").SetUserId(order.Userid)
                        .SetParams(new { OrderId = order.Id })
                        .SetClass(nameof(OrderShippedAutoCmdHandler))
                        .SetError(ex, ex is CustomResponseException cex ? cex.ErrorCode : Consts.Err.AutoShippedOrder_error));
                }
            }

            result.NotOkOrderIds = notOkOrderIds?.ToArray();
            result.Errs = errs?.Select(_ => _.Message).ToArray();
            return result;
        }        

    }
}
