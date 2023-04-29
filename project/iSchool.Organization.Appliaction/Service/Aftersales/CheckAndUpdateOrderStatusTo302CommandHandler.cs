using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class CheckAndUpdateOrderStatusTo302CommandHandler : IRequestHandler<CheckAndUpdateOrderStatusTo302Command>
    {
        OrgUnitOfWork _orgUnitOfWork;
        ILogger<CheckAndUpdateOrderStatusTo302CommandHandler> _logger;
        public CheckAndUpdateOrderStatusTo302CommandHandler(IOrgUnitOfWork orgUnitOfWork
            , ILogger<CheckAndUpdateOrderStatusTo302CommandHandler> logger)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _logger = logger;
        }
        public async Task<Unit> Handle(CheckAndUpdateOrderStatusTo302Command request, CancellationToken cancellationToken)
        {
            try
            {
                _orgUnitOfWork.BeginTransaction();
                string sql = @"UPDATE OrderDetial SET [status] = 302
WHERE 
NOT EXISTS(SELECT 1 FROM OrderRefunds WHERE [Status] NOT IN (5,6,13,16,17,20,21) AND IsValid = 1 AND OrderDetailId = OrderDetial.id)
AND
[status] = 303
AND 
id = @orderDetailId;
UPDATE [Order] SET [status] = 302
WHERE 
NOT EXISTS(SELECT 1 FROM OrderDetial WHERE [Status] IN (303) AND OrderDetial.orderid = [Order].id)
AND
[status] = 303
AND 
id = @orderId";
                await _orgUnitOfWork.ExecuteAsync(sql, new { orderDetailId = request.OrderDetailId, orderId = request.OrderId }, _orgUnitOfWork.DbTransaction);
                _orgUnitOfWork.CommitChanges();

            }
            catch (Exception ex)
            {
                _orgUnitOfWork.Rollback();
                _logger.LogError(ex, "尝试扭转订单状态到302失败。");
            
            }
            return Unit.Value;


        }
    }
}
