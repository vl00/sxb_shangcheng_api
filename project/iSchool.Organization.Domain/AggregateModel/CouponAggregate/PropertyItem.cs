using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    public class PropertyItem : ValueObject
    {
        public Guid PropId { get; set; }

        public string Name { get; set; }

        public int Sort { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return PropId;
        }
    }
}
