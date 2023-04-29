using CSRedis;
using Dapper;
using iSchool.Infras.Locks;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.Wechat;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class OrderShippedOkThenSettleCmdHandler : IRequestHandler<OrderShippedOkThenSettleCmd, object>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        private readonly CSRedisClient redis;
        private readonly IConfiguration _config;
        private readonly IServiceProvider services;

        public OrderShippedOkThenSettleCmdHandler(IOrgUnitOfWork orgUnitOfWork, CSRedisClient redis,
            IConfiguration _config,
            IMediator mediator, IServiceProvider services)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this.redis = redis;
            this._config = _config;
            this.services = services;
        }

        public async Task<object> Handle(OrderShippedOkThenSettleCmd cmd, CancellationToken cancellationToken)
        {
            var order = cmd.Order;
            if (order == null)
            {
                var orders = await _mediator.Send(new OrderDetailSimQuery { OrderId = cmd.OrderId });
                order = orders.Orders?.FirstOrDefault();
                if (order == null)
                    throw new CustomResponseException("无效的订单", Consts.Err.OrderIsNotValid_OnShipped);
            }
            if (((OrderStatusV2)order.OrderStatus) != OrderStatusV2.Shipped)
                throw new CustomResponseException("当前订单状态不能确定收货", Consts.Err.OrderStatus_IsNot_Shipping);

            var ver = order.Prods?.FirstOrDefault()?._Ver;
            var shippedOkTime = order.OrderUpdateTime;

            // 确认收货确定佣金有效
            IList<CourseDrpInfo> courseDrpinfos = default!;
            {
                var ls_params = new List<ApiDrpFxRequest.OrgOrderSettleCmd_param>();
                foreach (var courseOrderProdItemDto in order.Prods.OfType<CourseOrderProdItemDto>())
                {
                    var courseId = courseOrderProdItemDto.Id;
                    var goodsId = courseOrderProdItemDto.GoodsId;
                    //
                    var courseDrpinfo = courseDrpinfos?.FirstOrDefault(_ => _.Courseid == courseId);
                    if (courseDrpinfo == null)
                    {
                        courseDrpinfo = await _mediator.Send(new GetCourseFxSimpleInfoQuery { CourseId = courseId });
                        if (courseDrpinfo == null)
                        {
                            continue;
                        }
                        courseDrpinfos ??= new List<CourseDrpInfo>();
                        courseDrpinfos.Add(courseDrpinfo);
                    }
                    //
                    //var bonusLockEndTime = courseDrpinfo.NolimitType switch
                    //{
                    //    (int)NolimitTypeEnum.ExactDate => courseDrpinfo.NolimitAfterDate.Value.AddDays(1).Date.AddMilliseconds(-1),
                    //    // 不用order.UserPayTime了
                    //    (int)NolimitTypeEnum.NDaysLater => shippedOkTime.AddDays(courseDrpinfo.NolimitAfterBuyInDays.Value + 1).Date.AddMilliseconds(-1),
                    //    (int)NolimitTypeEnum.NotLocked => shippedOkTime.AddSeconds(-10),
                    //    _ => DateTime.Parse("1990-01-01"), //default,
                    //};
                    var receivingAfterDays = courseDrpinfo.ReceivingAfterDays ?? 3;
                    var bonusLockEndTime = cmd.IsFixUpTime ? shippedOkTime
                        : receivingAfterDays <= 0 ? shippedOkTime.AddSeconds(-10) : shippedOkTime.AddDays(receivingAfterDays + 1).Date.AddMilliseconds(-1);
                    //
                    ls_params.Add(new ApiDrpFxRequest.OrgOrderSettleCmd_param
                    {
                        OrderDetailId = courseOrderProdItemDto.OrderDetailId,
                        BonusLockEndTime = bonusLockEndTime,
                        _OrderId = order.OrderId,
                    });
                }
                if (ls_params.Count > 0)
                {
                    // 确定佣金有效
                    return await _mediator.Send(new ApiDrpFxRequest
                    {
                        Ctn = new ApiDrpFxRequest.OrgOrderSettleCmd { UserId = order.UserId, Param = ls_params }
                    });
                }
            }

            return default;
        }
    }
}
