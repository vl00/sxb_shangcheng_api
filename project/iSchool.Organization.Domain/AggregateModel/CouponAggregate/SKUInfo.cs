using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    public class BuySKU:ValueObject
    {
        public Guid SKUId { get; set; }

        public decimal UnitPrice { get; set; }

        public int Number { get; set; }

        public Guid BrandId { get; set; }

        public IEnumerable<int> GoodTypes { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return SKUId;
        }
    }
}
