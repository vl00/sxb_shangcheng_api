using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.Mini.RequestModels.Orders;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Mini.Services.Orders
{
    public class OrderPresentedPointsQueryHandler : IRequestHandler<OrderPresentedPointsQuery, int?>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;

        public OrderPresentedPointsQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<int?> Handle(OrderPresentedPointsQuery request, CancellationToken cancellationToken)
        {
            string sql = @"
 SELECT 
  Sum(CAST(
  case  CourseGoodPointCashBack.PointCashBackType 
	when 1 then
		case when (payment-ISNULL(JSON_VALUE(OrderDetial.ctn,'$.costprice'),0)) > 0 then (payment-ISNULL(JSON_VALUE(OrderDetial.ctn,'$.costprice'),0)) * (CAST(CourseGoodPointCashBack.PointCashBackValue AS decimaL) / 100) * 100 else 0 end 
	when 2 then 
		case when (payment-ISNULL(JSON_VALUE(OrderDetial.ctn,'$.costprice'),0)) * 100 >= CourseGoodPointCashBack.PointCashBackValue then CourseGoodPointCashBack.PointCashBackValue else 0 end
	else 0
    end 
	AS
	INT
   )) points
   FROM OrderDetial
  JOIN CourseGoodPointCashBack on OrderDetial.productid  = CourseGoodPointCashBack.GoodId
  where
  CourseGoodPointCashBack.IsValid = 1
  and 
  ISJSON(OrderDetial.ctn) = 1
  AND 
  OrderDetial.id = @OrderDetailId
  AND  ISNULL(Point,0) = 0
";
            return await _orgUnitOfWork.ExecuteScalarAsync<int?>(sql, new { OrderDetailId = request.OrderDetailId });
        }
    }
}
