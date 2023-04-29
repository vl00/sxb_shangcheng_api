using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class CouponInfoQueryHandler : IRequestHandler<CouponInfoQuery, CouponInfo>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public CouponInfoQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }
        public async Task<CouponInfo> Handle(CouponInfoQuery request, CancellationToken cancellationToken)
        {
            string sql = @"SELECT * FROM CouponInfo WHERE Id = @id";
            CouponInfo couponInfo = await _orgUnitOfWork.QueryFirstOrDefaultAsync<CouponInfo>(sql, new { id = request.Id });
            if (couponInfo == null) throw new KeyNotFoundException($"Id={request.Id},找不到该对象。");
            return couponInfo;
        }
    }
}
