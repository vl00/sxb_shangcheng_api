using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.IntegrationEvents;
using iSchool.Organization.Appliaction.IntegrationEvents.Events;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Event.Order;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static iSchool.Organization.Appliaction.Service.Aftersales.CheckAndUpdateOrderToRefundSuccessCommandHandler;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class CheckAndUpdateOrderToRefundSuccessCommandHandler : IRequestHandler<CheckAndUpdateOrderToRefundSuccessCommand, OrderRefundSuccessStatusChangeType>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
		ILogger<CheckAndUpdateOrderToRefundSuccessCommandHandler> _logger;
        public CheckAndUpdateOrderToRefundSuccessCommandHandler(IOrgUnitOfWork orgUnitOfWork
            , IMediator mediator
			, ILogger<CheckAndUpdateOrderToRefundSuccessCommandHandler> logger)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _mediator = mediator;
			_logger = logger;
        }

        public async Task<OrderRefundSuccessStatusChangeType> Handle(CheckAndUpdateOrderToRefundSuccessCommand request, CancellationToken cancellationToken)
        {
            OrderRefundSuccessStatusChangeType result = 0;
            _orgUnitOfWork.BeginTransaction();
            try
            {
                int changeOrderDetailStatus = await _orgUnitOfWork.ExecuteAsync("UPDATE OrderDetial SET [status] = 203 WHERE (ISNULL(RefundCount, 0) + ISNULL(ReturnCount, 0)) = number AND id = @OrderDetailId And [status] != 203 ;", new { request.OrderDetailId }, _orgUnitOfWork.DbTransaction);
                int changeOrderStatusFrom103 = await _orgUnitOfWork.ExecuteAsync(@"
UPDATE [Order] SET [status] = 203
WHERE
NOT EXISTS(SELECT 1 FROM OrderDetial WHERE [Order].id = OrderDetial.orderid AND OrderDetial.[status] != 203)
AND 
id = @OrderId
AND
status = 103
", new { request.OrderId }, _orgUnitOfWork.DbTransaction);
                if (changeOrderDetailStatus > 0)
                {
                    result |= OrderRefundSuccessStatusChangeType.OrderDetailStatusChangeToRefund;
                }
                if (changeOrderStatusFrom103 > 0)
                {
                    result |= OrderRefundSuccessStatusChangeType.OrderStatusChangeToRefundFrom103;
                }
                _orgUnitOfWork.CommitChanges();

                if (!result.HasFlag(OrderRefundSuccessStatusChangeType.OrderStatusChangeToRefundFrom103)) {
                    int changeOrderStatusFromOthers = await _orgUnitOfWork.ExecuteAsync(@"
UPDATE [Order] SET [status] = 203
WHERE
NOT EXISTS(SELECT 1 FROM OrderDetial WHERE [Order].id = OrderDetial.orderid AND OrderDetial.[status] != 203)
AND 
id = @OrderId
", new { request.OrderId }, _orgUnitOfWork.DbTransaction);
                    if (changeOrderStatusFromOthers > 0)
                    {
                        result |= OrderRefundSuccessStatusChangeType.OrderStatusChangeToRefundFromOthers;
                    }
                }



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"操作DB更新订单到203失败,OrderDetailId = {request.OrderDetailId}");    
                _orgUnitOfWork.Rollback();
            }

            
            if (result.HasFlag(OrderRefundSuccessStatusChangeType.OrderDetailStatusChangeToRefund))
            {
                //订单详情状态流转到退款状态时触发对应事件。
                await _mediator.Publish(new OrderDetailTransferToRefundStateDomainEvent(request.OrderDetailId));
            }

            return result;

        }

        [Flags]
        public enum OrderRefundSuccessStatusChangeType
        {
            OrderDetailStatusChangeToRefund = 1,
            //扭转退款状态是从103过去的
            OrderStatusChangeToRefundFrom103 = 2,
            //扭转退款状态是从其它状态过去的
            OrderStatusChangeToRefundFromOthers = 4,
        }
    }
}
