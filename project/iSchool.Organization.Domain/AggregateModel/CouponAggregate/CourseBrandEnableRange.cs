using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    /// <summary>
    /// 好物/课程品牌
    /// </summary>
    public class CourseBrandEnableRange: EnableRange
    {
        [JsonRequired]
        public Guid Id { get; set; }
        [JsonRequired]
        public string Name { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
