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
   public  class StaticApplyRefundAuditCountCommandHandler: IRequestHandler<StaticApplyRefundAuditCountCommand, int>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public StaticApplyRefundAuditCountCommandHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<int> Handle(StaticApplyRefundAuditCountCommand request, CancellationToken cancellationToken)
        {
            int applyRefundAuditCount  = await _orgUnitOfWork.ExecuteScalarAsync<int>(@"SELECT SUM([Count]) FROM OrderRefunds  WHERE OrderDetailId = @OrderDetailId AND [Status] NOT IN (6,13,16,20,21)  AND IsValid = 1", new { OrderDetailId = request.OrderDetailId },_orgUnitOfWork.DbTransaction);
            return applyRefundAuditCount;

        }
    }
}
