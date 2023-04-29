using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels.Orders;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.Lables;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Lables
{
    /// <summary>
    /// 查询订单封面
    /// </summary>
    public class OrderProductBannerQueryHandler : IRequestHandler<OrderProductBannerQuery, ResponseResult>
    {
        OrgUnitOfWork _unitOfWork;
        public OrderProductBannerQueryHandler(IOrgUnitOfWork unitOfWork)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
        }
        public async Task<ResponseResult> Handle(OrderProductBannerQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            //订单找出来
            string sql = $@" SELECT top 1 c.banner from [order] o join OrderDetial od on o.id=od.orderid left join CourseGoods g on od.productid=g.id   
left join Course c on c.id=g.courseid
  where o.AdvanceOrderId=@AdvanceOrderId
                      ;";


            var banners = _unitOfWork.Query<string>(sql, new { AdvanceOrderId = request.AdvanceOrderId }).FirstOrDefault();
            var tt = JsonConvert.DeserializeObject<List<string>>(banners);


            return ResponseResult.Success(tt);
        }
    }
}
