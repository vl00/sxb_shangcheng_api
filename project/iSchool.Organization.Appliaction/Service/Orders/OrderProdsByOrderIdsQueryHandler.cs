using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
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
    public class OrderProdsByOrderIdsQueryHandler : IRequestHandler<OrderProdsByOrderIdsQuery, OrderProdsByOrderIdsQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IMapper _mapper;
        IConfiguration _config;

        public OrderProdsByOrderIdsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<OrderProdsByOrderIdsQryResult> Handle(OrderProdsByOrderIdsQuery query, CancellationToken cancellation)
        {
            var result = new OrderProdsByOrderIdsQryResult();
            var orders = query.Orders;
            var sql = string.Empty;
            var orderProducts = result.OrderProducts;

            if (orders == null && query.OrderIds == null)
                return result;

            // fill orders
            if (orders == null)
            {
                sql = @"select id,type from [order] where id in @orderids";
                var arr = await _orgUnitOfWork.QueryAsync<(Guid, int)>(sql, new { orderids = query.OrderIds });
                orders = arr.Select(_ => (_.Item1, (OrderType)_.Item2)).ToArray();
            }

            // 'wx购买课程'类型的订单
            var orders2 = orders.Where(_ => _.OrderType >= OrderType.BuyCourseByWx).ToArray();
            if (orders2.Length > 0)
            {
                var prods2 = await Find_ty2_ctn2(orders2);
                if (prods2.Length > 0)
                    orderProducts = orderProducts.Union(prods2);

                if (orders2.Length != prods2.Length)
                {
                    var orders22 = orders2.Where(x => !prods2.Select(_ => _.OrderId).Contains(x.OrderId)).ToArray();
                    prods2 = await Find_ty2_ctn1(orders2);
                    if (prods2.Length > 0)
                        orderProducts = orderProducts.Union(prods2);
                }
            }

            result.OrderProducts = orderProducts.ToArray();
            return result;
        }

        // find in db
        async Task<(Guid OrderId, OrderProdItemDto[] Products)[]> Find_ty2_ctn1((Guid OrderId, OrderType OrderType)[] orders2)
        {
            var sql = $@"
select p.orderid,c.no as id_s,isnull(c.banner_s,c.banner) as banner,
p.producttype as ProdType,p.price,p.origprice,p.number as BuyCount,p.Payment,p.[name],g.id as goodsid,p.status,
c.id,c.title,c.subtitle,c.stock,p.id as OrderDetailId,c.NewUserExclusive,od.DiscountAmount CouponAmount,
o.id as orgid,o.[no] as orgid_s,o.[name] as orgname,o.logo,o.[authentication],o.[desc],o.subdesc
from [OrderDetial] p 
left join CourseGoods g on p.productid=g.id 
left join Course c on c.id=g.courseid
left join Organization o on o.id=c.orgid and o.IsValid=1
LEFT JOIN OrderDiscount od ON p.id = od.OrderId
where 1=1 and p.orderid in @orderids --p.producttype={ProductType.Course.ToInt()}
and c.id is not null and g.id is not null
";
            var arr = (await _orgUnitOfWork.QueryAsync<(Guid, long, string), CourseOrderProdItemDto, (Guid, long, string), CourseOrderProdItem_OrgItemDto, (Guid, OrderProdItemDto)>(sql,
                splitOn: "prodType,orgid,logo",
                param: new { orderids = orders2.Select(_ => _.OrderId) },
                map: (info1, courseProd, orgInfo, orgInfo2) =>
                {
                    courseProd.Id_s = UrlShortIdUtil.Long2Base32(info1.Item2);
                    courseProd.Banner = info1.Item3 == null ? new string[0] : info1.Item3.ToObject<string[]>();
                    courseProd.OrgInfo = orgInfo2;
                    courseProd.OrgInfo.Id = orgInfo.Item1;
                    courseProd.OrgInfo.Id_s = UrlShortIdUtil.Long2Base32(orgInfo.Item2);
                    courseProd.OrgInfo.Name = orgInfo.Item3;
                    courseProd.PropItemIds = Array.Empty<Guid>();
                    courseProd.PropItemNames = Array.Empty<string>();
                    courseProd.StatusDesc = ((OrderStatusV2)courseProd.Status).GetDesc();
                    if (courseProd.Payment <= 0) courseProd.Payment = courseProd.PricesAll;
                    return (info1.Item1, courseProd);
                }
            )).AsArray();

            var prods2 = arr.GroupBy(x => x.Item1, x => x.Item2).Select(x => (OrderId: x.Key, x.ToArray())).ToArray();

            // find goods's propitems
            if (prods2.Length > 0)
            {
                sql = $@"
select id, '['+(string_agg(idname,','+char(10)) within group(order by sort_pg,sort_i))+']'
from (select g.id,p.sort as sort_pg,pi.sort as sort_i,
(select pi.id,pi.name for json path,without_array_wrapper) as idname
from CourseGoods g
left join CourseGoodsPropItem gpi on gpi.goodsid=g.id
left join CoursePropertyItem pi on pi.id=gpi.propitemid
left join CourseProperty p on p.id=pi.propid
where g.id in @goodsIds
)T group by id
";
                var gjs = await _orgUnitOfWork.QueryAsync<(Guid, string)>(sql, new
                {
                    goodsIds = prods2.SelectMany(x => x.Item2.OfType<CourseOrderProdItemDto>().Select(_ => _.GoodsId)).Distinct(),
                });
                foreach (var (_0, ps) in prods2)
                {
                    foreach (var courseProd in ps.OfType<CourseOrderProdItemDto>())
                    {
                        if (!gjs.TryGetOne(out var gpi, _ => _.Item1 == courseProd.GoodsId)) continue;
                        var jarr = JArray.Parse(gpi.Item2);
                        courseProd.PropItemIds = jarr.Select(j => (Guid)j["id"]).ToArray();
                        courseProd.PropItemNames = jarr.Select(j => (string)j["name"]).ToArray();
                    }
                }
            }

            return prods2;
        }

        // find in '[ctn]' field (json)
        async Task<(Guid OrderId, OrderProdItemDto[] Products)[]> Find_ty2_ctn2((Guid OrderId, OrderType OrderType)[] orders2)
        {
            var sql = $@"
select p.OrderId,p.price,p.origprice,p.number,p.Payment,p.producttype,p.id,p.status,p.point,p.ctn,od.discountAmount from [OrderDetial] p 
LEFT JOIN OrderDiscount od ON p.id = od.OrderId
where 1=1 and p.orderid in @orderids 
";
            var arr = (await _orgUnitOfWork.QueryAsync<OrderDetial, string,decimal?, (Guid, OrderProdItemDto)>(sql,
                splitOn: "ctn,discountAmount",
                param: new { orderids = orders2.Select(_ => _.OrderId) },
                map: (orderDetial, ctn, couponAmount) =>
                {
                    var t = GetItemDtoByCtn(ctn, orderDetial.Producttype);
                    if (t != null)
                    {
                        t.OrderDetailId = orderDetial.Id;
                        t.Price = orderDetial.Price;
                        t.Origprice = orderDetial.Origprice;
                        t.BuyCount = orderDetial.Number;
                        if (t.Payment <= 0) t.Payment = t.PricesAll;
                        t.CouponAmount = couponAmount.GetValueOrDefault();
                        if (orderDetial.Point != null)
                        {
                            t.PointsInfo = new PointsExchangeInfo() { Points = orderDetial.Point.Value, Price = 0 };
                        }
                        
                    }
                    if (t is CourseOrderProdItemDto ct)
                    {
                        ct.Status = orderDetial.Status;
                        ct.StatusDesc = ((OrderStatusV2)ct.Status).GetDesc();
                    }
                    return (orderDetial.Orderid, t);
                }
            )).Where(_ => _.Item2 != null).AsArray();

            var prods2 = arr.GroupBy(x => x.Item1, x => x.Item2).Select(x => (OrderId: x.Key, x.ToArray())).ToArray();
            return prods2;
        }

        private OrderProdItemDto GetItemDtoByCtn(string ctn, int prodtype)
        {
            if (string.IsNullOrEmpty(ctn)) return null;
            switch ((ProductType)prodtype)
            {
                case ProductType.Course:
                case ProductType.Goodthing:
                    {
                        var p = ctn?.ToObject<CourseGoodsOrderCtnDto>();
                        var t = p == null ? null : _mapper.Map<CourseOrderProdItemDto>(p);
                        if (t.ProdType == default) t.ProdType = prodtype;
                        t._ctn = !ctn.IsNullOrEmpty() ? JObject.Parse(ctn) : null;
                        t.ProductId = t.GoodsId;
                        t.ProductTitle = t.Title;
                        return t;
                    }                    
            }
            return new OrderProdItemDto { ProdType = prodtype  };
        }
    }
}
