using AutoMapper;
using CSRedis;
using Dapper;
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
    public class MiniOrderDetailQueryHandler : IRequestHandler<MiniOrderDetailQuery, MiniOrderDetailQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;

        public MiniOrderDetailQueryHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<MiniOrderDetailQryResult> Handle(MiniOrderDetailQuery query, CancellationToken cancellation)
        {
            var sql = $@"
select AdvanceOrderId,userid,id as OrderId,code as OrderNo,[status] as OrderStatus,[type] as OrderType, totalpayment as Paymoney, paymenttime as UserPayTime, paymenttype as Paytype,
CreateTime as OrderCreateTime,ModifyDateTime as OrderUpdateTime,AppointmentStatus as BookingCourseStatus,
ExpressCode as ExpressNu, ExpressType as ExpressCompanyName,remark,
ShippingTime,SendExpressTime,
recvusername,[address],mobile as recvMobile,recvprovince as Province,recvcity as City,recvarea as Area
from dbo.[Order] 
where IsValid=1 {"and id=@OrderId".If(query.OrderId != default)} {"and code=@OrderNo".If(!query.OrderNo.IsNullOrEmpty())}
";
            var result = (await _orgUnitOfWork.QueryAsync<MiniOrderDetailQryResult, (DateTime?, DateTime?), RecvAddressDto, MiniOrderDetailQryResult>(sql, 
                param: new { query.OrderId, query.OrderNo },
                splitOn: "ShippingTime,recvusername",
                map: (rr, ts, address) => 
                {
                    rr.SendExpressTime = ts.Item1 == null && ts.Item2 == null ? (DateTime?)null
                        : ts.Item1 == null ? ts.Item2
                        : ts.Item2 == null ? ts.Item1
                        : ts.Item1.Value < ts.Item2.Value ? ts.Item1.Value : ts.Item2.Value;
                    rr.RecvAddressDto = address;
                    return rr;
                }
            )).FirstOrDefault();
            if (result == null)
            {
                throw new CustomResponseException("订单不存在.");
            }
            if (result.UserId != query.UserId)
            {
                //throw new CustomResponseException("暂无权限.");
            }
            result.PaytypeDesc = OrderHelper.GetPaytypeDesc0((PaymentType)result.Paytype);
            result.BookingCourseStatusDesc = result.BookingCourseStatus == null ? null : ((BookingCourseStatusEnum)result.BookingCourseStatus).GetDesc();

            // fix order status
            {
                var status = (OrderStatusV2)result.OrderStatus;
                if (status.In(OrderStatusV2.Unpaid, OrderStatusV2.Paiding) && (DateTime.Now - result.OrderCreateTime >= TimeSpan.FromMinutes(30)))
                {
                    status = OrderStatusV2.Cancelled;
                    result.OrderStatus = (int)status;

                    AsyncUtils.StartNew(new CheckOrderIsExpiredCommand());
                }
                result.OrderStatusDesc = OrderHelper.GetStatusDesc4Front(status);
            }

            // 查询课程信息
            result.Prods = (await _mediator.Send(new OrderProdsByOrderIdsQuery
            {
                Orders = new[] { (result.OrderId, (OrderType)result.OrderType) }
            })).OrderProducts
            .FirstOrDefault().Products ?? new OrderProdItemDto[0];

            // 物流信息
            if (result.ExpressNu != null)
            {
                var kd = await _mediator.Send(new GetOrderKuaidiDetailQuery { OrderId = query.OrderId });
                result.LastExpressDesc = (kd?.CompanyName == null ? "" : $"{kd?.CompanyName}: ") + kd?.Items?.FirstOrDefault()?.Desc;
                result.LastExpressTime = DateTime.TryParse(kd?.Items?.FirstOrDefault()?.Time, out var _time) ? _time : (DateTime?)null;
                result.ExpressCompanyName = kd?.CompanyName;
            }

            // 兑换码
            {
                var rc = (await _mediator.Send(new GetOrderRedeemInfoQueryArgs
                {
                    OrderIds = new[] { result.OrderId }
                })).FirstOrDefault();
                if (rc?.OrderId == result.OrderId)
                {
                    result.RedeemCode = rc.RedeemCode;
                    result.RedeemUrl = rc.Url;
                    result.RedeemMsg = rc.Msg;
                    result.RedeemIsRedirect = rc.IsRedirect;
                }
            }

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
