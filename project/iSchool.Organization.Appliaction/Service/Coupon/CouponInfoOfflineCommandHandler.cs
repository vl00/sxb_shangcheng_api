using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    class CouponInfoOfflineCommandHandler : IRequestHandler<CouponInfoOfflineCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public CouponInfoOfflineCommandHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<bool> Handle(CouponInfoOfflineCommand request, CancellationToken cancellationToken)
        {
            string sql = @"UPDATE [Organization].[dbo].[CouponInfo] SET [Status] = 0  WHERE Id=@id";
            return (await _orgUnitOfWork.ExecuteAsync(sql, new { id = request.Id })) > 0;

        }
    }
}
