using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Order
{
    public class OrdersByMobileQueryHandler : IRequestHandler<OrdersByMobileQuery, List<OrdersByMobileQueryResponse>>
    {
        OrgUnitOfWork _unitOfWork;
        IMediator _mediator;
        public OrdersByMobileQueryHandler(IOrgUnitOfWork unitOfWork,IMediator mediator)
        {
            _unitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
        }

        public async Task<List<OrdersByMobileQueryResponse>> Handle(OrdersByMobileQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            #region userInfos from usercenter
            string ordMobile = "";
            List<Guid> userIds = null;
            var dp = new DynamicParameters();
           
            if (!string.IsNullOrEmpty(request.OrderMobile) && !string.IsNullOrEmpty(request.RecvMobile))//并行
            {
                ordMobile = request.OrderMobile;
                dp.Set("mobile", request.RecvMobile);
                userIds = _unitOfWork.DbConnection.Query<Guid>(@$" select distinct userid from [dbo].[Order] where IsValid=1 and status in (
 { OrderStatusV2.Completed.ToInt()}
,{ OrderStatusV2.Paid.ToInt()}
,{ OrderStatusV2.Ship.ToInt()}
,{ OrderStatusV2.Shipping.ToInt()}
,{ OrderStatusV2.Shipped.ToInt()}
) and mobile=@mobile "
                    , dp).ToList();
                if (userIds.Any() == false) return null;
            }
            else if (!string.IsNullOrEmpty(request.OrderMobile))//下单人
            {
                ordMobile = request.OrderMobile;
            }
            else if (!string.IsNullOrEmpty(request.RecvMobile))//收货人
            {
                dp.Set("mobile", request.RecvMobile);
                userIds = _unitOfWork.DbConnection.Query<Guid>(@$" select distinct userid from [dbo].[Order] where IsValid=1 
and status in (
 {OrderStatusV2.Completed.ToInt()}
,{OrderStatusV2.Paid.ToInt()}
,{OrderStatusV2.Ship.ToInt()}
,{OrderStatusV2.Shipping.ToInt()}
,{OrderStatusV2.Shipped.ToInt()}
)

and mobile=@mobile "
                    ,dp).ToList();
                if (userIds.Any() == false) return null;
            }
            
            var userInfos = _mediator.Send(new UserInfosByPhonesQuery() { OrdMobile = ordMobile, UserIds = userIds }).Result;
            if (userInfos?.Any() == false)//用户信息不存在
            {
                return null;
            }
            #endregion

            
            string sqlOrders = $@"
select distinct ord.mobile,ord.recvUsername,ord.userid as UserId,c.id as courseid,c.title
,ord.payment as PayAmount,ord.code as OrderCode,ord.id as OrdeId,1 as OrderType,0 as PayDisccountAmount
,detial.number as goodsCount
from [dbo].[Order] as ord
left join [dbo].[OrderDetial] as detial on ord.id=detial.orderid 
left join [dbo].[CourseGoods] as goods on detial.productid=goods.Id and goods.IsValid=1
left join [dbo].[Course] as c on goods.Courseid=c.id and c.IsValid=1
where ord.IsValid=1 --and detial.producttype=1 
and ord.type>=2
and ord.status in (
 {OrderStatusV2.Completed.ToInt()}
,{OrderStatusV2.Paid.ToInt()}
,{OrderStatusV2.Ship.ToInt()}
,{OrderStatusV2.Shipping.ToInt()}
,{OrderStatusV2.Shipped.ToInt()}
)
and ord.userid in ('{string.Join("','", userInfos.Select(_ => _.UserId).ToList())}')
and exists (select * from [dbo].[BigCourse] as bc where bc.IsValid=1 and bc.courseid=goods.courseid)
                        ";
          
            var ordersDB= _unitOfWork.DbConnection.Query<SmallCourseOrder>(sqlOrders, dp).ToList();

            var response = new List<OrdersByMobileQueryResponse>();
            #region smallCourseInfo
            foreach (var u in userInfos)
            {                
                var smallOrder = ordersDB?.Where(_ => _.UserId == u.UserId).ToList();
                if (smallOrder?.Any() == true)
                {
                    var ord = new OrdersByMobileQueryResponse();
                    ord.SmallCoursesOrders = smallOrder;
                    ord.UserId = u.UserId;
                    ord.NickName = u.NickName;
                    response.Add(ord);
                }
            }
            #endregion
            return response;
        }
    }
}
