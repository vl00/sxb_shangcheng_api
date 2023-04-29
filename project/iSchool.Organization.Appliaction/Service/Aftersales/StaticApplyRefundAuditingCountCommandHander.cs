using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Aftersales;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Aftersales
{
    public class StaticApplyRefundAuditingCountCommandHander : IRequestHandler<StaticApplyRefundAuditingCountCommand, int>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public StaticApplyRefundAuditingCountCommandHander(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<int> Handle(StaticApplyRefundAuditingCountCommand request, CancellationToken cancellationToken)
        {
            int unAuditCompeleteNumber = await _orgUnitOfWork.ExecuteScalarAsync<int>(@"SELECT SUM([Count]) FROM OrderRefunds  WHERE OrderDetailId = @OrderDetailId AND [Status] IN (2,3,4,12,14,15)  AND IsValid = 1", new { OrderDetailId = request.OrderDetailId },_orgUnitOfWork.DbTransaction);
            return unAuditCompeleteNumber;

        }
    }
}
