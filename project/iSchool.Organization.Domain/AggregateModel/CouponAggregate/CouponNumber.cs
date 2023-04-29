using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    public class CouponNumber : ValueObject
    {
        long Number;

        public CouponNumber(long number)
        {
            Number = number;
        }
        public override string ToString()
        {
            return Number.ToString("D8");

        }

        public static CouponNumber GetCouponNumberFromNumber(long number)
        {
            return new CouponNumber(number);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Number;
        }
    }
}
