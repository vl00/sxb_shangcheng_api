using iSchool.Organization.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate
{
    public interface ICouponReceiveRepository: IRepository<CouponReceive>
    {
        CouponReceive Add(CouponReceive couponReceive);
        bool Update(CouponReceive couponReceive);

        Task<bool> UpdateAsync(CouponReceive couponReceive,params string[] fields);


        Task<CouponReceive> FindAsync(Guid id);

        Task<CouponReceive> FindFromOrderAsync(Guid orderId);


    }
}
