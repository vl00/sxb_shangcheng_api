using iSchool.Domain.Repository.Interfaces;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using iSchool.Infrastructure;
using Dapper;

namespace iSchool.Organization.Appliaction.Services
{
    public class OrderOrderRefundsByOrderIdsQueryHandler : IRequestHandler<OrderOrderRefundsByOrderIdsQuery, IEnumerable<OrderRefundsDto>>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;


        public OrderOrderRefundsByOrderIdsQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public Task<IEnumerable<OrderRefundsDto>> Handle(OrderOrderRefundsByOrderIdsQuery request, CancellationToken cancellationToken)
        {
            var refunds = _orgUnitOfWork.DbConnection.Query<OrderRefunds>("SELECT * FROM  dbo.OrderRefunds WHERE  OrderId IN @ids AND IsValid=1", new { ids = request.OrderIds });

            var res = refunds.Select(p => new OrderRefundsDto
            {
                Id = p.Id,
                ProductId = p.ProductId,
                Count = p.Count,
                OrderDetailId = p.OrderDetailId,
                OrderId = p.OrderId,
                Status = p.Status,
                Type = p.Type
            });

            return Task.FromResult(res);
        }

    }
}
