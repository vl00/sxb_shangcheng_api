using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class GetOrderMultipleKuaidisQueryHandler : IRequestHandler<GetOrderMultipleKuaidisQuery, (IEnumerable<OrderItemDto> OrderItems, string Qrcode)>
    {

        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IRepository<OrderLogistics> _orderLogistics;
        private readonly IMediator _mediator;
        private IConfiguration _config;


        public GetOrderMultipleKuaidisQueryHandler(IOrgUnitOfWork orgUnitOfWork, IRepository<OrderLogistics> orderLogistics
            , IMediator mediator, IConfiguration config)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _orderLogistics = orderLogistics;
            this._mediator = mediator;
            _config = config;

        }

        public async Task<(IEnumerable<OrderItemDto> OrderItems, string Qrcode)> Handle(GetOrderMultipleKuaidisQuery request, CancellationToken cancellationToken)
        {
            var res = new List<OrderItemDto>();
            var sql = @"SELECT logistics.*,orders.*,orderdetail.*,course.* FROM dbo.OrderLogistics AS logistics
LEFT JOIN dbo.OrderDetial AS orderdetail ON orderdetail.id = logistics.OrderDetailId
LEFT JOIN  dbo.[Order]  orders ON orders.id = logistics.OrderId
LEFT JOIN  dbo.CourseGoods goods ON   goods.Id = orderdetail.productid
LEFT JOIN  dbo.Course course ON  course.id=goods.Courseid
 WHERE logistics.IsValid = 1 AND logistics.OrderId = @orderid
 ORDER BY logistics.CreateTime DESC
";
            var data = _orgUnitOfWork.DbConnection.Query<OrderLogistics, iSchool.Organization.Domain.Order, OrderDetial, Domain.Course, OrderItemDto>(sql, (logistics, orders, detail, course) =>
            {
                var item = res.FirstOrDefault(p => p.ExpressNu == logistics.ExpressCode);

                var ctnData = JsonConvert.DeserializeObject<CourseGoodsOrderCtnDto>(detail.Ctn);
                if (item == null)
                {
                    item = new OrderItemDto()
                    {
                        LogisticsId = logistics.Id,
                        OrderId = logistics.OrderId,
                        ExpressNu = logistics.ExpressCode,
                        ExpressCompanyName = logistics.ExpressType,
                        SendExpressTime = logistics.SendExpressTime,

                    };

                    item.Prods = new CourseOrderProdItemDto[] {
                        new CourseOrderProdItemDto {
                            Banner = new string[] { ctnData.Banner },
                            Id = ctnData.Id,
                            OrderDetailId = detail.Id,
                            GoodsId = detail.Productid,
                            ProdType = detail.Producttype,
                            BuyCount = logistics.Number,
                            Title = ctnData.Title,
                            Subtitle = ctnData.Subtitle,
                            PropItemNames = ctnData.PropItemNames,
                            SupplierInfo = new CourseOrderProdItem_SupplierInfo { Id = ctnData.SupplierId },
                            OrgInfo = new CourseOrderProdItem_OrgItemDto{
                                 Name = ctnData.OrgName
                            },
                            Price = detail.Price,
                            Origprice = detail.Origprice
                        }
                    };

                    res.Add(item);
                }
                else
                {
                    //存在
                    var prods = item.Prods.ToList();
                    prods.Add(new CourseOrderProdItemDto
                    {
                        Banner = new string[] { ctnData.Banner },
                        Id = ctnData.Id,
                        OrderDetailId = detail.Id,
                        GoodsId = detail.Productid,
                        ProdType = detail.Producttype,
                        BuyCount = logistics.Number,
                        Title = ctnData.Title,
                        Subtitle = ctnData.Subtitle,
                        PropItemNames = ctnData.PropItemNames,
                        SupplierInfo = new CourseOrderProdItem_SupplierInfo { Id = ctnData.SupplierId },
                        OrgInfo = new CourseOrderProdItem_OrgItemDto
                        {
                            Name = ctnData.OrgName
                        },
                        Price = detail.Price,
                        Origprice = detail.Origprice
                    });
                    item.Prods = prods.ToArray();
                }
                return null;
            }, new { orderid = request.OrderId });


            //获取最新物流
            do
            {
                var nus = res.Select(p => (p.ExpressNu, p.ExpressCompanyName)).AsArray();
                if (nus.Length < 1) break;
                var rr = await _mediator.Send(new GetLastKdnusDescQueryArgs { Nus = nus });
                foreach (var item in res)
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
            while (false);


            //查询没有发货的商品
            //剩余的detail 
            var surplusSql = @"SELECT orders.*,detail.*,course.*,
ISNULL(detail.number - ISNULL((SELECT SUM(Number) FROM dbo.OrderLogistics where  IsValid=1 and OrderDetailId = detail.id), 0),0) as surplus
 FROM dbo.OrderDetial AS detail
LEFT JOIN dbo.[Order] orders ON detail.orderid = orders.id
LEFT JOIN dbo.CourseGoods goods ON goods.Id = detail.productid
LEFT JOIN dbo.Course course ON course.id = goods.Courseid

WHERE detail.orderid = @orderid
AND ISNULL(detail.number-ISNULL((SELECT SUM(Number) FROM dbo.OrderLogistics where IsValid=1 and OrderDetailId = detail.id),0),0)> 0
";
            OrderItemDto orderItem = null;
            var surplusData = _orgUnitOfWork.DbConnection.Query<Domain.Order, OrderDetial, Domain.Course, int, object>(surplusSql, (Order, detail, course, num) =>
             {
                 var ctnData = JsonConvert.DeserializeObject<CourseGoodsOrderCtnDto>(detail.Ctn);
                 if (orderItem == null)
                 {
                     orderItem = new OrderItemDto()
                     {
                         Prods = new CourseOrderProdItemDto[]
                         {
                              new CourseOrderProdItemDto
                              {
                                  Banner=new string[]{ ctnData.Banner },
                                  Id = ctnData.Id,
                                  OrderDetailId = detail.Id,
                                  GoodsId = detail.Productid,
                                  ProdType = detail.Producttype,
                                  BuyCount=num,
                                  Title=ctnData.Title,
                                  Subtitle=ctnData.Subtitle,
                                  PropItemNames=ctnData.PropItemNames,
                                  SupplierInfo = new CourseOrderProdItem_SupplierInfo { Id = ctnData.SupplierId },
                                  OrgInfo=new CourseOrderProdItem_OrgItemDto{
                                       Name=ctnData.OrgName
                                  },
                                  Price=course.Price.Value,
                                  Origprice=course.Origprice
                              }
                         }
                     };
                 }
                 else
                 {
                     var prods = orderItem.Prods.ToList();
                     prods.Add(new CourseOrderProdItemDto
                     {
                         Banner = new string[] { ctnData.Banner },
                         Id = ctnData.Id,
                         OrderDetailId = detail.Id,
                         GoodsId = detail.Productid,
                         ProdType = detail.Producttype,
                         BuyCount = num,
                         Title = ctnData.Title,
                         Subtitle = ctnData.Subtitle,
                         PropItemNames = ctnData.PropItemNames,
                         SupplierInfo = new CourseOrderProdItem_SupplierInfo { Id = ctnData.SupplierId },
                         OrgInfo = new CourseOrderProdItem_OrgItemDto
                         {
                             Name = ctnData.OrgName
                         },
                         Price = course.Price.Value,
                         Origprice = course.Origprice
                     });
                     orderItem.Prods = prods.ToArray();
                 }
                 return null;
             }, new { orderid = request.OrderId }, _orgUnitOfWork.DbTransaction, true, "id,id,id,surplus");

            if (orderItem != null)
            {
                res.Add(orderItem);
            }


            // 小助手二维码

            var path = Path.Combine(Directory.GetCurrentDirectory(), _config[$"AppSettings:org_assistant"]);
            var bys = await File.ReadAllBytesAsync(path);
            var Qrcode = $"data:image/png;base64,{Convert.ToBase64String(bys)}";



            return (res, Qrcode);

        }
    }
}
