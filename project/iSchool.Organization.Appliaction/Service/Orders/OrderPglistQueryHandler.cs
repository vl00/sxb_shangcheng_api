using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
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
    [Obsolete]
    public class OrderPglistQueryHandler : IRequestHandler<OrderPglistQuery, OrderPglistQueryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;

        public OrderPglistQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<OrderPglistQueryResult> Handle(OrderPglistQuery query, CancellationToken cancellation)
        {
            var result = new OrderPglistQueryResult();
            await default(ValueTask);

            // OrderStatusArr
            {
                result.OrderStatusArr = new NameCodeDto<int>[]
                {
                    new NameCodeDto<int> { Name = "全部", Code = 0 },
                    new NameCodeDto<int> { Name = OrderStatusV2.Paid.GetDesc(), Code = OrderStatusV2.Paid.ToInt() },
                    new NameCodeDto<int> { Name = OrderStatusV2.Shipped.GetDesc(), Code = OrderStatusV2.Shipped.ToInt() },
                };
            }

            var sql = $@"select * from (
select o.id as orderid, o.code as orderno, o.type as ordertype, o.totalpayment as paymoney,
(case when o.status in ('{OrderStatusV2.Unpaid.ToInt()}','{OrderStatusV2.Paiding.ToInt()}') and datediff(second,o.CreateTime,getdate())>=60*30
then {OrderStatusV2.Cancelled.ToInt()}
else o.status
end) as orderstatus,
--(case when o.status={OrderStatusV2.Paid.ToInt()} then o.paymenttime
--when o.status in ('{OrderStatusV2.Unpaid.ToInt()}','{OrderStatusV2.Paiding.ToInt()}') and datediff(second,o.CreateTime,getdate())<60*30 
--then o.CreateTime
--when o.status in ('{OrderStatusV2.Unpaid.ToInt()}','{OrderStatusV2.Paiding.ToInt()}') and datediff(second,o.CreateTime,getdate())>=60*30 
--then dateadd(second,30*60,o.CreateTime)
--else o.ModifyDateTime
--end) as OrderUpdateTime,
o.CreateTime,
o.status as orderstatus0
--,o.paymenttime,o.ModifyDateTime
from [order] o where o.IsValid=1 and o.type>={OrderType.BuyCourseByWx.ToInt()}
{(query.Status == 0 ? "" :
query.Status == OrderStatusV2.Unpaid.ToInt() ? "and o.status=@Status and datediff(second,o.CreateTime,getdate())<60*30" :
"and o.status=@Status")}
{"and o.userid=@UserId".If(query.UserId != null)}
)T where 1=1 {$"and orderstatus not in ('{OrderStatusV2.Unpaid.ToInt()}')".If(true)} 
";            
            sql = $@"
select count(1) from ({sql}) T
;
{sql}
order by CreateTime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";

            var gr = await _orgUnitOfWork.DbConnection.QueryMultipleAsync(sql, query);
            var cc = await gr.ReadFirstAsync<int>();
            var items = (gr.Read<OrderItemDto, int, (OrderItemDto, int)>(
                splitOn: "orderstatus0", 
                func: (item, ts0) => 
                {
                    return (item, ts0);
                })
            ).AsArray();

            // 判断order status
            {
                var need_fix_expired = false;
                foreach (var (item, ts0) in items)
                {
                    var status0 = (OrderStatusV2)ts0; // 原order-status
                    var status = (OrderStatusV2)item.OrderStatus;
                    item.OrderStatusDesc = status.GetDesc();

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

            // 获取产品s
            {
                var produs = (await _mediator.Send(new OrderProdsByOrderIdsQuery
                {
                    Orders = items.Select(_ => (_.Item1.OrderId, (OrderType)_.Item1.OrderType)).ToArray()
                })).OrderProducts;

                foreach (var it in items)
                {
                    if (!produs.TryGetOne(out var p, _ => _.OrderId == it.Item1.OrderId)) continue;
                    it.Item1.Prods = p.Products;
                }
            }

            result.PageInfo = items.Select(_ => _.Item1).AsArray().ToPagedList(query.PageSize, query.PageIndex, cc);
            return result;
        }

    }
}
