using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Dapper;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class OrderRefundQueryHandler : IRequestHandler<OrderRefundQuery, OrderRefunds>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public OrderRefundQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }
        public async Task<OrderRefunds> Handle(OrderRefundQuery request, CancellationToken cancellationToken)
        {
            //目前使用的主从数据库，主库同步可能需要一定的时间，所以需要做重试。目前认为同步延迟最坏情况是3s。
            
            var orderRefunds = await Policy.HandleResult<OrderRefunds>(orderRefunds => orderRefunds == null).WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1) }).ExecuteAsync(async () =>
            {
                //查单个的时候使用 写库查。
                return await _orgUnitOfWork.DbConnection.QueryFirstOrDefaultAsync<OrderRefunds>("SELECT * FROM OrderRefunds WHERE Id=@Id", new { Id = request.OrderRefundId },_orgUnitOfWork.DbTransaction);
            });

            if (orderRefunds == null)
            {
                throw new KeyNotFoundException($"id=[{request.OrderRefundId}],找不到该对象信息。");
            }
            return orderRefunds;
        }
    }
}
