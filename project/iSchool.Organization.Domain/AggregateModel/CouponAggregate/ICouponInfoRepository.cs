using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using iSchool.Organization.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    public interface ICouponInfoRepository:IRepository<CouponInfo>
    {
        CouponInfo Add(CouponInfo couponInfo);
        Task<bool> UpdateAsync(CouponInfo couponInfo);

        Task<bool> UpdateAsync(CouponInfo couponInfo, params string[] fields);


        Task<CouponInfo> GetAsync(Guid id);

        Task<CouponInfo> FindFromNumberAsync(int number);
    }
}
