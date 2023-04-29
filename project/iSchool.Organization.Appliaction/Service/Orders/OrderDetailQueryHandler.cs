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
    public class OrderDetailQueryHandler : IRequestHandler<OrderDetailQuery, OrderDetailQueryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;

        public OrderDetailQueryHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<OrderDetailQueryResult> Handle(OrderDetailQuery query, CancellationToken cancellation)
        {
            var sql = $@"
select userid,id as OrderId,code as OrderNo,[status] as OrderStatus,[type] as OrderType, totalpayment as Paymoney, payment as Paymoney0, paymenttime as UserPayTime, paymenttype,
CreateTime as OrderCreateTime,ModifyDateTime as OrderUpdateTime,BeginClassMobile,
[address],recvusername,mobile as recvMobile,recvprovince as Province,recvcity as City,recvarea as Area,age
from dbo.[Order] 
where IsValid=1 {"and id=@OrderId".If(query.OrderId != default)} {"and code=@OrderNo".If(!query.OrderNo.IsNullOrEmpty())}
";
            var result = await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<OrderDetailQueryResult>(sql, new { query.OrderId, query.OrderNo });
            if (result == null)
            {
                throw new CustomResponseException("订单不存在.");
            }
            if (result.UserId != query.UserId)
            {
                //throw new CustomResponseException("暂无权限.");
            }

            // fix order status
            {
                var status = (OrderStatusV2)result.OrderStatus;
                if (status.In(OrderStatusV2.Unpaid, OrderStatusV2.Paiding) && (DateTime.Now - result.OrderCreateTime >= TimeSpan.FromMinutes(30)))
                {
                    status = OrderStatusV2.Cancelled;
                    result.OrderStatus = (int)status;

                    AsyncUtils.StartNew(new CheckOrderIsExpiredCommand());
                }
                result.OrderStatusDesc = status.GetDesc();
            }

            // 查询课程信息
            result.Prods = (await _mediator.Send(new OrderProdsByOrderIdsQuery
            {
                Orders = new[] { (result.OrderId, (OrderType)result.OrderType) }
            })).OrderProducts
            .FirstOrDefault().Products ?? new OrderProdItemDto[0];

            // 小助手二维码
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), _config[$"AppSettings:org_assistant"]);
                var bys = await File.ReadAllBytesAsync(path);
                result.Qrcode = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }

            return result;
        }

    }
}
