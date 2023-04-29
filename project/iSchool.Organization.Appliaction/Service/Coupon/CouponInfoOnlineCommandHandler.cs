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
    class CouponInfoOnlineCommandHandler : IRequestHandler<CouponInfoOnlineCommand, bool>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public CouponInfoOnlineCommandHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }

        public async Task<bool> Handle(CouponInfoOnlineCommand request, CancellationToken cancellationToken)
        {
            string sql = @"UPDATE [Organization].[dbo].[CouponInfo] SET [Status] = 1  WHERE Id=@id";
            return (await _orgUnitOfWork.ExecuteAsync(sql, new { id = request.Id })) > 0;

        }
    }
}
