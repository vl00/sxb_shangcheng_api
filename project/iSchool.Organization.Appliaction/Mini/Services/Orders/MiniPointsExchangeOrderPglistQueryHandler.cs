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
using iSchool.Organization.Appliaction.ViewModels;
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
    public class MiniPointsExchangeOrderPglistQueryHandler : IRequestHandler<MiniPointsExchangeOrderPglistQuery, MiniPointsExchangeOrderPglistQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;

        public MiniPointsExchangeOrderPglistQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<MiniPointsExchangeOrderPglistQryResult> Handle(MiniPointsExchangeOrderPglistQuery query, CancellationToken cancellation)
        {

            var result = new MiniPointsExchangeOrderPglistQryResult();
            await default(ValueTask);
            string where = " ISNULL(totalpoints,0) >0 and userid = @UserId and  [status] >= 103 and [status] != 203";
            var sql = $@"
select count(1) from ( SELECT  
CASE WHEN status='{OrderStatusV2.Unpaid.ToInt()}' or status='{OrderStatusV2.Cancelled.ToInt()}'  THEN AdvanceOrderNo ELSE code END AS OrderCode,
CASE WHEN  status='{OrderStatusV2.Unpaid.ToInt()}' or status='{OrderStatusV2.Cancelled.ToInt()}'  THEN 1 ELSE 0 END AS  OrderStatusType
from
[order]   where {where} GROUP BY
CASE WHEN  status='{OrderStatusV2.Unpaid.ToInt()}' or status='{OrderStatusV2.Cancelled.ToInt()}'   THEN AdvanceOrderNo ELSE code END,
CASE WHEN  status='{OrderStatusV2.Unpaid.ToInt()}' or status='{OrderStatusV2.Cancelled.ToInt()}'  THEN 1 ELSE 0 END) t;

select  o.recvProvince,o.recvCity,o.recvArea,o.recvPostalcode, o.id as orderid, o.OrderCode as orderno, o.type as ordertype, o.amount as paymoney,o.OrderStatusType as orderstatustype,
(case when  o.status in ('{OrderStatusV2.Unpaid.ToInt()}','{OrderStatusV2.Paiding.ToInt()}')  and datediff(second,o.addtime,getdate())>=60*30
then {OrderStatusV2.Cancelled.ToInt()}
else o.status
end) as orderstatus,o.IsMultipleExpress,
o.CreateTime,o.RefundTime,o.ModifyDateTime as OrderUpdateTime,o.ExpressCode as ExpressNu,o.SendExpressTime,o.AdvanceOrderNo,o.AdvanceOrderId,o.totalPoints
,o.status as orderstatus0,o.ExpressType as Comcode
FROM
(
SELECT * FROM  
(SELECT  
CASE WHEN status='{OrderStatusV2.Unpaid.ToInt()}' or status='{OrderStatusV2.Cancelled.ToInt()}'  THEN AdvanceOrderNo ELSE code END AS OrderCode,
CASE WHEN  status='{OrderStatusV2.Unpaid.ToInt()}' or status='{OrderStatusV2.Cancelled.ToInt()}'  THEN 1 ELSE 0 END AS  OrderStatusType,
MAX(CreateTime) addtime,SUM(totalpayment) amount  
FROM dbo.[Order]
WHERE  
{where}
GROUP BY
CASE WHEN  status='{OrderStatusV2.Unpaid.ToInt()}' or status='{OrderStatusV2.Cancelled.ToInt()}'   THEN AdvanceOrderNo ELSE code END,
CASE WHEN  status='{OrderStatusV2.Unpaid.ToInt()}' or status='{OrderStatusV2.Cancelled.ToInt()}'  THEN 1 ELSE 0 END
ORDER BY addtime DESC OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
) a 
 LEFT JOIN dbo.[Order] AS orders  ON
 CASE 
 WHEN a.OrderStatusType=1 AND a.OrderCode=orders.AdvanceOrderNo THEN 1 
 WHEN a.OrderStatusType=0 AND a.OrderCode=orders.code THEN 1
 ELSE 0
 END =1
 ) o  ORDER BY  o.CreateTime DESC
";

            var gr = await _orgUnitOfWork.QueryMultipleAsync(sql, query);
            var cc = await gr.ReadFirstAsync<int>();
            var items = (gr.Read<OrderItemDto, int, string, (OrderItemDto, int, string)>(
                splitOn: "orderstatus0,Comcode",
                func: (item, ts0, comcode) =>
                {
                    return (item, ts0, comcode);
                })
            ).AsArray();

            // 判断order status
            {
                var need_fix_expired = false;
                foreach (var (item, ts0, _) in items)
                {
                    var status0 = (OrderStatusV2)ts0; // 原order-status
                    var status = (OrderStatusV2)item.OrderStatus;
                    item.OrderStatusDesc = OrderHelper.GetStatusDesc4Front(status);

                    if (status0.In(OrderStatusV2.Unpaid, OrderStatusV2.Paiding) && status == OrderStatusV2.Cancelled)
                    {
                        need_fix_expired = true;
                    }
                }
                if (need_fix_expired)
                {
                    AsyncUtils.StartNew(new CheckOrderIsExpiredCommand());
                }
            }

            // 退款金额
            foreach (var (item, ts0, _) in items)
            {
                var status = (OrderStatusV2)item.OrderStatus;
                if (item.OrderType == (int)OrderType.BuyCourseByWx && status == OrderStatusV2.RefundOk)
                    item.RefundMoney = item.Paymoney;
            }



            // 获取产品s
            {
                var produs = (await _mediator.Send(new OrderProdsByOrderIdsQuery
                {
                    Orders = items.Select(_ => (_.Item1.OrderId, (OrderType)_.Item1.OrderType)).ToArray()
                })).OrderProducts;

                //查询订单的退款情况
                var resfunds = await _mediator.Send(new OrderOrderRefundsByOrderIdsQuery
                {
                    OrderIds = items.Select(p => p.Item1).Select(p => p.OrderId).ToArray()
                });


                foreach (var it in items)
                {
                    if (!produs.TryGetOne(out var p, _ => _.OrderId == it.Item1.OrderId)) continue;


                    //补充退款中数量   已退款数量
                    foreach (var product in p.Products)
                    {
                        //detail 所有的退款情况
                        var detailRefunds = resfunds.Where(p => p.OrderDetailId == product.OrderDetailId);

                        if (detailRefunds.Count() == 0)
                            continue;

                        //已退款
                        var refundc = detailRefunds.Where(p =>
                           p.Type == (int)RefundTypeEnum.FastRefund
                           || p.Type == (int)RefundTypeEnum.BgRefund
                           || (p.Type == (int)RefundTypeEnum.Refund && p.Status == (int)RefundStatusEnum.RefundSuccess)
                           || (p.Type == (int)RefundTypeEnum.Return && p.Status == (int)RefundStatusEnum.ReturnSuccess));

                        product.RefundedCount = refundc.Sum(p => p.Count);

                        //退款中
                        //--查询审核失败数量
                        var FailedCount = detailRefunds.Where(p =>
                        p.Status == (int)RefundStatusEnum.InspectionFailed
                          || p.Status == (int)RefundStatusEnum.RefundAuditFailed
                          || p.Status == (int)RefundStatusEnum.ReturnAuditFailed
                          || p.Status == (int)RefundStatusEnum.Cancel
                          || p.Status == (int)RefundStatusEnum.CancelByExpired).Sum(p => p.Count);

                        product.RefundingCount = detailRefunds.Sum(p => p.Count) - FailedCount - product.RefundedCount;
                    }

                    it.Item1.Prods = p.Products;
                }
            }

            // 快递最新信息
            do
            {
                var nus = items.Select(_ => (_.Item1.ExpressNu, _.Item3)).Where(_ => _.ExpressNu != null).AsArray();
                if (nus.Length < 1) break;
                var rr = await _mediator.Send(new GetLastKdnusDescQueryArgs { Nus = nus });
                foreach (var x in items)
                {
                    if (x.Item1.ExpressNu == null) continue;
                    if (rr.TryGetOne(out var kd, (_) => _.Nu == x.Item1.ExpressNu && _.Comcode == x.Item3))
                    {
                        var kdcom = (await _mediator.Send(KuaidiServiceArgs.GetCode(kd.Comcode))).GetResult<KdCompanyCodeDto>();
                        x.Item1.ExpressCompanyName = kdcom?.Com;
                        x.Item1.LastExpressDesc = (kdcom?.Com == null ? "" : $"{kdcom?.Com}: ") + (!kd.Desc.IsNullOrEmpty() ? kd.Desc : "正在等待快递员上门揽收");
                        x.Item1.LastExpressTime = kd.Time;
                    }
                    else
                    {
                        var kdcom = (await _mediator.Send(KuaidiServiceArgs.GetCode(x.Item3))).GetResult<KdCompanyCodeDto>();
                        x.Item1.ExpressCompanyName = kdcom?.Com;
                        x.Item1.LastExpressDesc = (kdcom?.Com == null ? "" : $"{kdcom?.Com}: ") + "正在等待快递员上门揽收";
                        x.Item1.LastExpressTime = x.Item1.SendExpressTime;
                    }
                }
            }
            while (false);

            // 兑换码
            {
                var redcodes = await _mediator.Send(new GetOrderRedeemInfoQueryArgs
                {
                    OrderIds = items.Select(_ => _.Item1.OrderId).ToArray()
                });
                foreach (var x in items)
                {
                    if (!redcodes.TryGetOne(out var rc, (_) => _.OrderId == x.Item1.OrderId)) continue;
                    x.Item1.RedeemCode = rc.RedeemCode;
                    x.Item1.RedeemUrl = rc.Url;
                    x.Item1.RedeemMsg = rc.Msg;
                    x.Item1.RedeemIsRedirect = rc.IsRedirect;
                }
            }
            //组合待支付和关闭的订单
            var r = new List<OrderItemDto>();
            foreach (var it in items)
            {
                if (it.Item1.OrderStatusType == 0) r.Add(it.Item1);
                else
                {
                    if (!r.Exists(x => x.OrderNo == it.Item1.OrderNo))
                    {
                        var orders = items.Where(x => x.Item1.OrderNo == it.Item1.OrderNo).Select(x => x.Item1);

                        foreach (var order in orders)
                        {
                            if (it.Item1.OrderId != order.OrderId)
                                it.Item1.Prods = it.Item1.Prods.Union(order.Prods).AsArray();
                        }
                        r.Add(it.Item1);
                    }


                }
            }


            result.PageInfo = r.ToPagedList(query.PageSize, query.PageIndex, cc);
            return result;
        }

    }
}
