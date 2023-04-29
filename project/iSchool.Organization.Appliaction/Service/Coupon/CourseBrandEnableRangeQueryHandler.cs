using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
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
    public class CourseBrandEnableRangeQueryHandler : IRequestHandler<CourseBrandEnableRangeQuery, IEnumerable<CourseBrandEnableRange>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public CourseBrandEnableRangeQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }
        public async Task<IEnumerable<CourseBrandEnableRange>> Handle(CourseBrandEnableRangeQuery request, CancellationToken cancellationToken)
        {

            string sql = @"
SELECT [id],[name]
  FROM [Organization].[dbo].[Organization]
  WHERE [name] LIKE @text
  ORDER BY id
  OFFSET @offset ROWS
  FETCH NEXT @limit ROWS ONLY";

            return await _orgUnitOfWork.QueryAsync<CourseBrandEnableRange>(sql, new { text = $"%{request.Text}%", offset = request.Offset, limit = request.Limit });

        }
    }
}
