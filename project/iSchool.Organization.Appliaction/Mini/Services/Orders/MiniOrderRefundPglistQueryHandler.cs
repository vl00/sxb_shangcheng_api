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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class MiniOrderRefundPglistQueryHandler : IRequestHandler<MiniOrderRefundPglistQuery, MiniOrderPglistQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;

        public MiniOrderRefundPglistQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<MiniOrderPglistQryResult> Handle(MiniOrderRefundPglistQuery query, CancellationToken cancellation)
        {
            var result = new MiniOrderPglistQryResult();
            await default(ValueTask);

            // OrderStatusArr
            {
                result.OrderStatusArr = new NameCodeDto<int>[]
                {
                    new NameCodeDto<int> { Name = "全部", Code = 0 },
                    new NameCodeDto<int> { Name = OrderStatusV2.Unpaid.GetDesc(), Code = OrderStatusV2.Unpaid.ToInt() },
                    new NameCodeDto<int> { Name = OrderStatusV2.Ship.GetDesc(), Code = OrderStatusV2.Ship.ToInt() },
                    new NameCodeDto<int> { Name = OrderStatusV2.Shipping.GetDesc(), Code = OrderStatusV2.Shipping.ToInt() },
                    new NameCodeDto<int> { Name = OrderStatusV2.Completed.GetDesc(), Code = OrderStatusV2.Completed.ToInt() },
                };
            }

            //            var sql = $@"
            //select count(1) from [order] o where 1=1 {"and o.userid=@UserId".If(query.UserId != null)} and (o.status={OrderStatusV2.RefundOk.ToInt()} or o.IsPartialRefund=1)

            //select o.recvProvince,o.recvCity,o.recvArea,o.recvPostalcode, o.id as orderid, o.code as orderno, o.type as ordertype, o.totalpayment as paymoney,
            //o.status as orderstatus,
            //o.CreateTime,o.RefundTime,o.ModifyDateTime as OrderUpdateTime,o.ExpressCode as ExpressNu,o.SendExpressTime,o.AdvanceOrderNo,o.AdvanceOrderId
            //,o.status as orderstatus0,o.ExpressType as Comcode
            //FROM [order] o
            //where 1=1 {"and o.userid=@UserId".If(query.UserId != null)} and (o.status={OrderStatusV2.RefundOk.ToInt()} or o.IsPartialRefund=1)
            //order by o.RefundTime desc
            //OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
            //";

            //            var gr = await _orgUnitOfWork.DbConnection.QueryMultipleAsync(sql, query);
            //            var cc = await gr.ReadFirstAsync<int>();
            //            var items = (gr.Read<OrderItemDto, int, string, (OrderItemDto, int, string)>(
            //                splitOn: "orderstatus0,Comcode",
            //                func: (item, ts0, comcode) =>
            //                {
            //                    return (item, ts0, comcode);
            //                })
            //            ).AsArray();

            //            // 判断order status
            //            {
            //                foreach (var (item, ts0, _) in items)
            //                {
            //                    var status0 = (OrderStatusV2)ts0; // 原order-status
            //                    var status = (OrderStatusV2)item.OrderStatus;
            //                    item.OrderStatusDesc = OrderHelper.GetStatusDesc4Front(status);
            //                }
            //            }

            //            // 获取产品s
            //            {
            //                var produs = (await _mediator.Send(new OrderProdsByOrderIdsQuery
            //                {
            //                    Orders = items.Select(_ => (_.Item1.OrderId, (OrderType)_.Item1.OrderType)).ToArray()
            //                })).OrderProducts;

            //                foreach (var it in items)
            //                {
            //                    if (!produs.TryGetOne(out var p, _ => _.OrderId == it.Item1.OrderId)) continue;
            //                    it.Item1.Prods = p.Products.OfType<CourseOrderProdItemDto>().Where(_ => _.Status == (int)OrderStatusV2.RefundOk).ToArray();
            //                }
            //            }

            //            // 退款金额
            //            foreach (var (item, _, _) in items)
            //            {
            //                item.RefundMoney = item.Prods.Sum(_ => _.PricesAll);
            //            }


            var sql = @"SELECT orders.id OrderId,AdvanceOrderNo,AdvanceOrderId,orders.code OrderNo,
            orders.status OrderStatus ,orders.type OrderType,refunds.CreateTime OrderRefundApplyTime,
            StepOneTime,refunds.RefundTime,refunds.
            CreateTime OrderRefundApplyTime,refunds.Type RefundType,refunds.Status RefundStatus ,refunds.SendBackAddress,
            refunds.Id RefundId,refunds.Code RefundCode,SendBackExpressType ExpressCompanyName,SendBackExpressCode ExpressNu,refunds.StepOneTime,
            orderdetail.ctn,orderdetail.Price as goodprice,orderdetail.number buycount,refunds.Count RefundCount,refunds.Price RefundMoney
             FROM dbo.OrderRefunds AS refunds
            LEFT JOIN dbo.OrderDetial AS orderdetail ON  refunds.OrderDetailId=orderdetail.id
            LEFT JOIN dbo.[Order] AS orders ON orders.id=orderdetail.orderid
            LEFT JOIN  dbo.CourseGoods AS good ON orderdetail.productid=good.Id
            WHERE refunds.IsValid=1 AND orders.Creator=@UserId  ORDER BY refunds.CreateTime desc
            OFFSET (@PageIndex-1)*@PageSize ROWS  FETCH NEXT @PageSize ROWS ONLY  ";


            var res = _orgUnitOfWork.Query<OrderItemDto, DateTime?, string, decimal, short, short, decimal, OrderItemDto>(sql, (res, steponetime, ctn, price, buycount, refundcount, refundmoney) =>
                 {
                     if (!string.IsNullOrEmpty(ctn))
                     {
                         var ctnData = JsonConvert.DeserializeObject<CourseGoodsOrderCtnDto>(ctn);
                         if (steponetime != null)

                             res.StepOneTime = steponetime.Value.UnixTicks();
                         res.Prods = new CourseOrderProdItemDto[] {
                              new CourseOrderProdItemDto{
                                   Price=price,
                                   BuyCount=buycount,
                                   Banner=new string[]{ ctnData.Banner },
                                   Id=ctnData.Id,
                                   Title=ctnData.Title,
                                   Subtitle=ctnData.Subtitle,
                                   PropItemNames=ctnData.PropItemNames,
                                   OrgInfo=new CourseOrderProdItem_OrgItemDto{
                                        Name=ctnData.OrgName
                                   },
                                   RefundCount=refundcount,
                                   RefundMoney=refundmoney
                              }
                         };
                         if (res.Prods != null && res.Prods.Length > 0)
                         {
                             res.RefundMoney = res.Prods.Sum(p => p.RefundMoney);
                         }
                     }

                     return res;
                 }, query, _orgUnitOfWork.DbTransaction, true, "StepOneTime,ctn,goodprice,buycount,RefundCount,RefundMoney").ToList();

            var countSql = @"SELECT COUNT(1) FROM dbo.OrderRefunds refunds
LEFT JOIN dbo.[Order] orders ON refunds.OrderId = orders.id
WHERE refunds.IsValid = 1 AND orders.Creator = @UserId ";


            var cc = _orgUnitOfWork.QueryFirstOrDefault<int>(countSql, query);

            //如果用户退货并寄回商品查询快递最新信息
            do
            {

                var nus = res.Where(item => item.RefundType == (int)RefundTypeEnum.Return && item.RefundStatus == (int)RefundStatusEnum.Receiving).Select(p => (p.ExpressNu, p.ExpressCompanyName)).AsArray();
                if (nus.Length < 1) break;
                var rr = await _mediator.Send(new GetLastKdnusDescQueryArgs { Nus = nus });

                foreach (var item in res)
                {
                    if (item.RefundType == (int)RefundTypeEnum.Return && item.RefundStatus == (int)RefundStatusEnum.Receiving)
                    {
                        if (rr.TryGetOne(out var kd, (_) => _.Nu == item.ExpressNu && _.Comcode == item.ExpressCompanyName))
                        {
                            var kdcom = (await _mediator.Send(KuaidiServiceArgs.GetCode(kd.Comcode))).GetResult<KdCompanyCodeDto>();
                            item.ExpressCompanyName = kdcom?.Com;
                            item.LastExpressDesc = (kdcom?.Com == null ? "" : $"{kdcom?.Com}: ") + (!kd.Desc.IsNullOrEmpty() ? kd.Desc : "正在等待快递员上门揽收");
                            item.LastExpressTime = kd.Time;
                        }
                        else
                        {
                            var kdcom = (await _mediator.Send(KuaidiServiceArgs.GetCode(item.ExpressCompanyName))).GetResult<KdCompanyCodeDto>();
                            item.ExpressCompanyName = kdcom?.Com;
                            item.LastExpressDesc = (kdcom?.Com == null ? "" : $"{kdcom?.Com}: ") + "正在等待快递员上门揽收";
                            item.LastExpressTime = item.SendExpressTime;
                        }
                    }
                }
            }
            while (false);

            //// 兑换码
            //{
            //    var redcodes = await _mediator.Send(new GetOrderRedeemInfoQueryArgs
            //    {
            //        OrderIds = items.Select(_ => _.Item1.OrderId).ToArray()
            //    });
            //    foreach (var x in items)
            //    {
            //        if (!redcodes.TryGetOne(out var rc, (_) => _.OrderId == x.Item1.OrderId)) continue;
            //        x.Item1.RedeemCode = rc.RedeemCode;
            //        x.Item1.RedeemUrl = rc.Url;
            //        x.Item1.RedeemMsg = rc.Msg;
            //        x.Item1.RedeemIsRedirect = rc.IsRedirect;
            //    }
            //}

            #region 
            ////组合待支付和关闭的订单
            //var r = new List<OrderItemDto>();
            //foreach (var it in items)
            //{
            //    if (it.Item1.OrderStatusType == 0) r.Add(it.Item1);
            //    else
            //    {
            //        if (!r.Exists(x => x.OrderNo == it.Item1.OrderNo))
            //        {
            //            var orders = items.Where(x => x.Item1.OrderNo == it.Item1.OrderNo).Select(x => x.Item1);

            //            foreach (var order in orders)
            //            {
            //                if (it.Item1.OrderId != order.OrderId)
            //                    it.Item1.Prods=it.Item1.Prods.Union(order.Prods).AsArray();
            //            }
            //            r.Add(it.Item1);
            //        }


            //    }
            //}
            #endregion

            result.PageInfo = res.ToPagedList(query.PageSize, query.PageIndex, cc);
            return result;
        }

    }
}
