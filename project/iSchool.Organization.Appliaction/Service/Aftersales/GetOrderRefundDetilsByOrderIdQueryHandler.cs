using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Appliaction.ViewModels.Aftersales;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class GetOrderRefundDetilsByOrderIdQueryHandler : IRequestHandler<GetOrderRefundDetilsByOrderIdQuery, List<OrderRefundDetail>>
    {

        OrgUnitOfWork _orgUnitOfWork;
        public GetOrderRefundDetilsByOrderIdQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<List<OrderRefundDetail>> Handle(GetOrderRefundDetilsByOrderIdQuery request, CancellationToken cancellationToken)
        {
            string sql = @"SELECT 
Course.id CourseID
, Course.title CourseTitle
,Course.subtitle CourseSubTitle
,Course.banner Banners 
,Course.banner_s BannerThumbnails
,OrderDetial.price Price
,OrderRefunds.Price RefundAmount
,OrderRefunds.IsContainFreight 
,OrderRefunds.[Count] RefundCount
,OrderRefunds.[Status] RefundStatus FROM OrderDetial
JOIN OrderRefunds ON OrderRefunds.OrderDetailId = OrderDetial.ID
JOIN CourseGoods ON CourseGoods.Id =  OrderDetial.productid
JOIN Course ON Course.id = CourseGoods.Courseid
WHERE 
(OrderRefunds.[Status] != 20  And OrderRefunds.[Status] != 21 And OrderRefunds.[Status] != 13  And OrderRefunds.[Status] != 16 And OrderRefunds.[Status] != 6 And OrderRefunds.IsValid =1)
And
OrderDetial.orderid= @orderId";
            var result = await _orgUnitOfWork.QueryAsync(sql, new { orderId = request.OrderId });


            return Map2OrderRefundDetail(result);
        }


        List<OrderRefundDetail> Map2OrderRefundDetail(IEnumerable<dynamic> results)
        {

            return results.Select(s =>
             {
                 return new OrderRefundDetail()
                 {
                     CourseID = s.CourseID,
                     CourseTitle = s.CourseTitle,
                     CourseSubTitle = s.CourseSubTitle,
                     Banners = BannerJsonToList(s.Banners),
                     BannerThumbnails = BannerJsonToList(s.BannerThumbnails),
                     RefundCount = s.RefundCount,
                     Price = s.Price,
                     RefundAmount = s.RefundAmount,
                     RefundStatus = ExplainRefundStatus((RefundStatusEnum)s.RefundStatus),
                     IsContainFreight = s.IsContainFreight
                 };

             })?.ToList();

        }

        List<string> BannerJsonToList(string json)
        {
            if (string.IsNullOrEmpty(json)) return new List<string>();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);

        }

        string ExplainRefundStatus(RefundStatusEnum refundStatusEnum)
        {
            if (refundStatusEnum != RefundStatusEnum.RefundSuccess && refundStatusEnum != RefundStatusEnum.ReturnSuccess)
                return "退款中";
            return "已退款";
        }
    }
}
