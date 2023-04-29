using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Appliaction.RequestModels.Orders;
using iSchool.Organization.Appliaction.ResponseModels.Orders;
using MediatR;
using iSchool.Infrastructure;
using iSchool.Organization.Domain;
using System.Linq;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Domain.Enum;

namespace iSchool.Organization.Appliaction.OrgService_bg.Order
{
    /// <summary>
    /// 根据订单Id获取订单详情[后台]
    /// </summary>
    public class OrderDetailsByOrdIdQueryHandler : IRequestHandler<OrderDetailsByOrdIdQuery, List<OrderDetailsDto>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        public OrderDetailsByOrdIdQueryHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _mediator = mediator;
        }

        public Task<List<OrderDetailsDto>> Handle(OrderDetailsByOrdIdQuery request, CancellationToken cancellationToken)
        {
            string sql = $@"
select c.banner,org.[name] as OrgName,c.title,c.subtitle,det.productid goodid,OCAMT.CouponAmount,det.Payment
,det.price,det.Origprice,det.number,det.Point
,ord.code,ord.totalpayment,ord.totalPoints,ord.freight,det.[status],ord.status as OrderStatus,convert(varchar,ord.CreateTime,120) as CreateTime
,ord.userid,'' as Usermobile,'' as Nickname,det.number as detailcount,det.id as detailid
,ord.recvUsername,ord.mobile as recvmobile,ord.[address],ord.[recvProvince],ord.[recvCity],ord.[recvArea],ord.Remark
,kd.LastJStr,kd.CompanyName,logistics.expressCode,logistics.expressType,ord.appointmentStatus,ex.code as dhcode
,logistics.number as logistaicsCount
from dbo.[Order] as ord 
left join dbo.OrderDetial as det on ord.id=det.orderid 
LEFT JOIN (SELECT OrderDetial.id,SUM(OrderDiscount.DiscountAmount) CouponAmount FROM  OrderDetial JOIN OrderDiscount ON OrderDiscount.OrderId = OrderDetial.id GROUP BY OrderDetial.id) OCAMT ON OCAMT.ID = det.ID
left join dbo.Course as c on det.courseid=c.id and c.IsValid=1
left join dbo.Organization as org on c.orgid= org.id and org.IsValid=1
left join dbo.Exchange as ex on ex.orderid=ord.id and ex.IsValid=1
LEFT JOIN  dbo.OrderLogistics logistics ON logistics.OrderDetailId = det.id and logistics.IsValid=1
left join dbo.KuaidiNuData as kd on kd.Nu=logistics.expressCode and kd.Company=logistics.expressType
where ord.IsValid=1 and ord.id='{request.OrderId}' order by logistics.SendExpressTime desc;
";

            var response = new List<OrderDetailsDto>();

            //其他数据（部分退款，待发货）
            OrderDetailsDto otherData = null;


            var res = _orgUnitOfWork.Query<GoodsData, OrderDetailsDto, short?, (Guid detailid, short? detailcount, short? logistaicsCount, int? status, GoodsData goods, OrderDetailsDto detail)>(sql, (goods, data, logistaicsCount) =>
                  {

                      if (string.IsNullOrEmpty(data.ExpressCode))
                      {
                          if (otherData == null)
                          {
                              otherData = data;
                              goods.Number = data.DetailCount;
                              //goods.TotalPayment = goods.Price * data.DetailCount;
                              goods.Banner = goods.Banner?.Replace("\\", "");
                              otherData.GoodsDatas = new List<GoodsData> { goods };
                          }
                          else
                          {
                              goods.Number = data.DetailCount;
                              //goods.TotalPayment =  goods.Price * data.DetailCount;
                              goods.Banner = goods.Banner?.Replace("\\", "");
                              otherData.GoodsDatas.Add(goods);
                          }
                      }
                      else
                      {
                          var item = response.FirstOrDefault(p => p.ExpressCode == data.ExpressCode);
                          if (item == null)
                          {
                              item = data;
                              goods.Number = logistaicsCount ?? 0;
                              //goods.TotalPayment = goods.Price * logistaicsCount;
                              goods.Banner = goods.Banner?.Replace("\\", "");

                              item.GoodsDatas = new List<GoodsData>() { goods };
                              response.Add(item);
                          }
                          else
                          {
                              goods.Number = logistaicsCount ?? 0;
                              //goods.TotalPayment = goods.Price * logistaicsCount;
                              goods.Banner = goods.Banner?.Replace("\\", "");

                              item.GoodsDatas.Add(goods);
                          }
                      }
                      return (data.DetailId, data.DetailCount, logistaicsCount, data.Status, goods, data);
                  },"", _orgUnitOfWork.DbTransaction, true, "OrgName,code,logistaicsCount").ToList();


            //遍历数据  （排查退款，未发货的数据）

            res.Where(p => p.status != (int)OrderStatusV2.RefundOk)
                .GroupBy(p => p.detailid)
                .Select(g => new { DetailId = g.Key, Count = g.Sum(c => c.logistaicsCount) })
                .ToList()
                .ForEach(c =>
            {
                var temp = res.FirstOrDefault(p => p.detailid == c.DetailId);
                if (temp.detailcount - c.Count > 0 && c.Count != 0)
                {
                    if (otherData == null)
                    {
                        otherData = temp.detail.Clone();
                        otherData.CompanyName = null;
                        otherData.ExpressCode = null;
                        otherData.LastJStr = null;
                        otherData.Status = 0;
                        otherData.StatusDec = null;
                        otherData.GoodsDatas = new List<GoodsData>();
                    }
                    var good = temp.goods.Clone();
                    good.Number = temp.detailcount - c.Count ?? 0;
                    good.TotalPayment = (temp.detailcount - c.Count) * good.Price;
                    good.Banner = good.Banner?.Replace("\\", "");

                    if (otherData.GoodsDatas == null)
                        otherData.GoodsDatas = new List<GoodsData>();

                    otherData.GoodsDatas.Add(good);
                }
            });

            if (otherData != null)
            {
                response.Add(otherData);
            }

            if (response == null || response.Count == 0) return Task.FromResult(new List<OrderDetailsDto>());


            var user = _mediator.Send(new UserInfosByUserIdsOrMobileQuery() { OrdMobile = "", UserIds = new List<Guid>() { response.FirstOrDefault().UserId } }).Result.FirstOrDefault();


            response.ForEach((item) =>
                {
                    item.UserMobile = user?.Mobile;
                    item.Nickname = user?.NickName;
                    item.StatusDec = ((OrderStatusV2)item.Status).GetDesc();
                    item.AppointmentStatus = string.IsNullOrEmpty(item.AppointmentStatus) ? "" :
                    int.TryParse(item.AppointmentStatus, out int status) ?
                    ((BookingCourseStatusEnum)(status)).GetDesc() : item.AppointmentStatus;
                    //item.Banner = string.IsNullOrEmpty(item.Banner) ? null : JsonSerializationHelper.JSONToObject<List<string>>(item.Banner)?.FirstOrDefault();
                    item.LastJStr = string.IsNullOrEmpty(item.LastJStr) ? null : JsonSerializationHelper.JSONToObject<LastKDMsg>(item.LastJStr)?.Desc;
                });

            return Task.FromResult(response);
        }
    }


    /// <summary>
    /// 快递最新信息
    /// </summary>
    public class LastKDMsg
    {
        public DateTime Time { get; set; }

        public string Desc { get; set; }
    }

}
