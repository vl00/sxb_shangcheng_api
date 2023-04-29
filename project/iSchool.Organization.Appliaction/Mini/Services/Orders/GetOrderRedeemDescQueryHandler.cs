using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class GetOrderRedeemDescQueryHandler : IRequestHandler<GetOrderRedeemDescQuery, GetOrderRedeemDescQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        IUserInfo me;
        CSRedisClient redis;              

        public GetOrderRedeemDescQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;            
        }

        public async Task<GetOrderRedeemDescQryResult> Handle(GetOrderRedeemDescQuery query, CancellationToken cancellation)
        {
            var result = new GetOrderRedeemDescQryResult();

            (Guid orderId, string orderNo) = (default, default);
            orderId = Guid.TryParse(query.OrderStr, out var _oid) ? _oid : default;
            orderNo = orderId == default ? query.OrderStr : default;

            var order = await _mediator.Send(new MiniOrderDetailQuery { OrderId = orderId, OrderNo = orderNo });
            result.OrderId = order.OrderId;
            result.OrderNo = order.OrderNo;
            result.OrderStatus = order.OrderStatus;
            result.OrderCreateTime = order.OrderCreateTime;
            result.RedeemCode = order.RedeemCode;
            result.RedeemUrl = order.RedeemUrl;
            result.RedeemMsg = order.RedeemMsg;
            result.RedeemIsRedirect = order.RedeemIsRedirect;
            result.Prods = order.Prods;

            var courseId = result.Prods.FirstOrDefault() is CourseOrderProdItemDto courseProdItemDto ? courseProdItemDto.Id : default;

            // 购前须知
            if (courseId != default)
            {
                var sql = @"
select * from CourseNotices where CourseId=@courseId and IsValid=1
order by sort
";
                var notices = await _orgUnitOfWork.QueryAsync<CourseNoticeItem>(sql, new { courseId });
                result.CourseNotices = notices.AsArray();
                if (result.CourseNotices.Length < 1) result.CourseNotices = null;
            }

            return result;
        }

    }
}
