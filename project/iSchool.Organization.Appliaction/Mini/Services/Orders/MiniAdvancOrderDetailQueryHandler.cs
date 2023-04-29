using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.Queries;
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
    public class MiniAdvancOrderDetailQueryHandler : IRequestHandler<MiniAdvanceOrderDetailQuery, MiniAdvanceOrderDetailQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;
        ICouponQueries _couponQueries;
        public MiniAdvancOrderDetailQueryHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IMapper mapper, ICouponQueries couponQueries)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
            _couponQueries = couponQueries;
        }

        public async Task<MiniAdvanceOrderDetailQryResult> Handle(MiniAdvanceOrderDetailQuery query, CancellationToken cancellation)
        {
            var isold = false;
            var result = new MiniAdvanceOrderDetailQryResult();
            var where = "";
            //兼容旧的查询
            if (null != query.OrderId && Guid.Empty != query.OrderId)
            {
                where = "and id=@OrderId";
                isold = true;
            }
            else
            {
                where = $"{ "and AdvanceOrderNo=@AdvanceOrderNo".If(query.AdvanceOrderNo.ToLower().StartsWith("oga")) }   { "and Code=@AdvanceOrderNo".If(query.AdvanceOrderNo.ToLower().StartsWith("ogc"))}";
                result.AdvanceOrderNo = query.AdvanceOrderNo;
            }


            if (where.IsNullOrEmpty()) return result;
            var sql = $@"
select IsMultipleExpress,freight,userid,AdvanceOrderNo,AdvanceOrderId,id as OrderId,code as OrderNo,[status] as OrderStatus,[type] as OrderType, totalpayment as Paymoney, paymenttime as UserPayTime, paymenttype as Paytype,
CreateTime as OrderCreateTime,ModifyDateTime as OrderUpdateTime,AppointmentStatus as BookingCourseStatus,
ExpressCode as ExpressNu, ExpressType as ExpressCompanyName,remark,totalPoints,
ShippingTime,SendExpressTime,
recvusername,[address],mobile as recvMobile,recvprovince as Province,recvcity as City,recvarea as Area
from dbo.[Order] 
where IsValid=1 {where}
";/*{"and id=@OrderId".If(query.OrderId != default)} {"and code=@OrderNo".If(!query.OrderNo.IsNullOrEmpty())}*/
            var orders = (await _orgUnitOfWork.QueryAsync<OrderAdvanceDetailQryResult, (DateTime?, DateTime?), RecvAddressDto, OrderAdvanceDetailQryResult>(sql,
                param: new { query.AdvanceOrderNo, query.OrderId },
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
            ));
            var testOrder = orders.FirstOrDefault();
            if (testOrder == null)
            {
                throw new CustomResponseException("订单不存在.");
            }
            if (testOrder.UserId != query.UserId)
            {
                //throw new CustomResponseException("暂无权限.");
            }
            if (testOrder.UserId != query.UserId)
            {
                //throw new CustomResponseException("暂无权限.");
            }
            if (isold)
            {
                result.AdvanceOrderNo = testOrder.AdvanceOrderNo;
            }


            foreach (var o in orders)
            {
                o.PaytypeDesc = OrderHelper.GetPaytypeDesc0((PaymentType)o.Paytype);
                o.BookingCourseStatusDesc = o.BookingCourseStatus == null ? null : ((BookingCourseStatusEnum)o.BookingCourseStatus).GetDesc();

                // fix order status
                {
                    var status = (OrderStatusV2)o.OrderStatus;
                    if (status.In(OrderStatusV2.Unpaid, OrderStatusV2.Paiding) && (DateTime.Now - o.OrderCreateTime >= TimeSpan.FromMinutes(30)))
                    {
                        status = OrderStatusV2.Cancelled;
                        o.OrderStatus = (int)status;

                        AsyncUtils.StartNew(new CheckOrderIsExpiredCommand());
                    }
                    o.OrderStatusDesc = OrderHelper.GetStatusDesc4Front(status);
                }
                // 查询课程信息
                o.Prods = (await _mediator.Send(new OrderProdsByOrderIdsQuery
                {
                    Orders = new[] { (o.OrderId, (OrderType)o.OrderType) }
                })).OrderProducts
                .FirstOrDefault().Products ?? new OrderProdItemDto[0];

                //补充退款中数量   已退款数量
                var resfunds = await _mediator.Send(new OrderOrderRefundsByOrderIdsQuery
                {
                    OrderIds = new Guid[] { o.OrderId }
                });
                foreach (var prod in o.Prods)
                {
                    //detail 所有的退款情况
                    var detailRefunds = resfunds.Where(p => p.OrderDetailId == prod.OrderDetailId);
                    if (detailRefunds.Count() == 0)
                        continue;
                    //已退款

                    var refundc = detailRefunds.Where(p =>
                     p.Type == (int)RefundTypeEnum.FastRefund
                     || p.Type == (int)RefundTypeEnum.BgRefund
                     || (p.Type == (int)RefundTypeEnum.Refund && p.Status == (int)RefundStatusEnum.RefundSuccess)
                     || (p.Type == (int)RefundTypeEnum.Return && p.Status == (int)RefundStatusEnum.ReturnSuccess));


                    prod.RefundedCount = refundc.Sum(p => p.Count);


                    //退款中
                    //--查询审核失败数量
                    var FailedCount = detailRefunds.Where(p =>
                    p.Status == (int)RefundStatusEnum.InspectionFailed
                      || p.Status == (int)RefundStatusEnum.RefundAuditFailed
                      || p.Status == (int)RefundStatusEnum.ReturnAuditFailed
                      || p.Status == (int)RefundStatusEnum.Cancel
                      || p.Status == (int)RefundStatusEnum.CancelByExpired).Sum(p => p.Count);

                    prod.RefundingCount = detailRefunds.Sum(p => p.Count) - FailedCount - prod.RefundedCount;
                }







                // 物流信息--新的物流select OrderLogistics
                //if (o.ExpressNu != null)
                //{
                //var kd = await _mediator.Send(new GetOrderKuaidiDetailQuery { OrderId = o.OrderId });
                //o.LastExpressDesc = (kd?.CompanyName == null ? "" : $"{kd?.CompanyName}: ") + kd?.Items?.FirstOrDefault()?.Desc;
                //o.LastExpressTime = DateTime.TryParse(kd?.Items?.FirstOrDefault()?.Time, out var _time) ? _time : (DateTime?)null;
                //o.ExpressCompanyName = kd?.CompanyName;
                //}

                //如果是单物流查询最新物流信息出来
                //多物流进入物流查询页
                if (o.IsMultipleExpress == false && o.ExpressNu != null)
                {
                    var kd = await _mediator.Send(new GetOrderKuaidiDetailQuery { OrderId = o.OrderId });
                    o.LastExpressDesc = (kd?.CompanyName == null ? "" : $"{kd?.CompanyName}: ") + kd?.Items?.FirstOrDefault()?.Desc;
                    o.LastExpressTime = DateTime.TryParse(kd?.Items?.FirstOrDefault()?.Time, out var _time) ? _time : (DateTime?)null;
                    o.ExpressCompanyName = kd?.CompanyName;
                }



                // 兑换码
                {
                    var rc = (await _mediator.Send(new GetOrderRedeemInfoQueryArgs
                    {
                        OrderIds = new[] { o.OrderId }
                    })).FirstOrDefault();
                    if (rc?.OrderId == o.OrderId)
                    {
                        o.RedeemCode = rc.RedeemCode;
                        o.RedeemUrl = rc.Url;
                        o.RedeemMsg = rc.Msg;
                        o.RedeemIsRedirect = rc.IsRedirect;
                    }
                }
            }

            result.Orders = orders.AsArray();
            var randomOrder = orders.FirstOrDefault();
            result.PayTime = randomOrder?.UserPayTime;
            result.TotalPayAmount = orders.Sum(x => x.Paymoney);
            result.TotalPoints = orders.Sum(x => x.TotalPoints);

            // 小助手二维码
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), _config[$"AppSettings:org_assistant"]);
                var bys = await File.ReadAllBytesAsync(path);
                result.Qrcode = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }

            
            if (orders != null && orders.Any())
            {
                var orderCouponInfos = await _couponQueries.GetOrderUseCouponInfosAsync(orders.First().AdvanceOrderNo, query.OrderId);
                if (orderCouponInfos.Any())
                {
                    foreach (var order in result.Orders)
                    {
                        var orderCouponInfo = orderCouponInfos.FirstOrDefault(o => o.OrderId == order.OrderId);
                        order.CouponInfo = orderCouponInfo;

                    }

                    result.CouponInfoAggregation = new MiniAdvanceOrderDetailQryResult.CouponUseInfo()
                    {
                        CouponAmount = orderCouponInfos.Sum(o=>o.CouponAmount),
                        CouponName = orderCouponInfos.First().CouponName
                    };
                }

            }

            return result;
        }

    }
}
