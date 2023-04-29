using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    public class GoodsTypeEnableRange: EnableRange
    {

        [JsonRequired]
        public int Id { get; set; }
        [JsonRequired]
        public string Name { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
