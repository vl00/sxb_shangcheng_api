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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class OrderDetailSimQueryHandler : IRequestHandler<OrderDetailSimQuery, OrderDetailSimQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;

        public OrderDetailSimQueryHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<OrderDetailSimQryResult> Handle(OrderDetailSimQuery query, CancellationToken cancellation)
        {
            var result = new OrderDetailSimQryResult();

            var sql = $@"
select userid,id as OrderId,code as OrderNo,AdvanceOrderNo,AdvanceOrderId,[status] as OrderStatus,[type] as OrderType, totalpayment as Paymoney, payment as Paymoney0, paymenttime as UserPayTime, paymenttype,
CreateTime as OrderCreateTime,ModifyDateTime as OrderUpdateTime,BeginClassMobile,
[address],recvusername,mobile as recvMobile,recvprovince as Province,recvcity as City,recvarea as Area,age
,totalpoints
from dbo.[Order] 
where IsValid=1 {"and AdvanceOrderId=@AdvanceOrderId".If(query.AdvanceOrderId != default)} {"and AdvanceOrderNo=@AdvanceOrderNo".If(!query.AdvanceOrderNo.IsNullOrEmpty())}
{"and Id=@OrderId".If(query.OrderId != default)} {"and code=@OrderNo".If(!query.OrderNo.IsNullOrEmpty())}
";
            var orders = (await (query.UseReadConn ? _orgUnitOfWork.ReadDbConnection : _orgUnitOfWork.DbConnection)
                .QueryAsync<OrderDetailQueryResult>(sql, new { query.AdvanceOrderNo, query.AdvanceOrderId, query.OrderId, query.OrderNo })).AsArray();
            if (orders.Length < 1)
            {
                //throw new CustomResponseException("订单不存在.", 404);
                return result;
            }

            // fix order status
            var order = orders[0];
            if (!query.IgnoreCheckExpired)
            {
                var status = (OrderStatusV2)order.OrderStatus;
                if (status.In(OrderStatusV2.Unpaid, OrderStatusV2.Paiding) && (DateTime.Now - order.OrderCreateTime >= TimeSpan.FromMinutes(30)))
                {
                    status = OrderStatusV2.Cancelled;
                    foreach (var o in orders)
                        o.OrderStatus = status.ToInt();

                    AsyncUtils.StartNew(new CheckOrderIsExpiredCommand { AdvanceOrderId = order.AdvanceOrderId });
                }
            }
            foreach (var o in orders)
            {
                o.OrderStatusDesc = ((OrderStatusV2)o.OrderStatus).GetDesc();
            }

            // 商品信息
            var prods = (await _mediator.Send(new OrderProdsByOrderIdsQuery
            {
                Orders = orders.Select(_ => (_.OrderId, (OrderType)_.OrderType)).ToArray()
            })).OrderProducts;

            foreach (var o in orders)
            {
                if (!prods.TryGetOne(out var p, _ => _.OrderId == o.OrderId)) continue;
                o.Prods = p.Products;
            }

            result.Orders = orders;
            result.AdvanceOrderId = order.AdvanceOrderId;
            result.AdvanceOrderNo = order.AdvanceOrderNo;
            return result;
        }

    }
}
