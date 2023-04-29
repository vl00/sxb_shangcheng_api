using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    public class SKUEnableRange : EnableRange
    {
        public bool IsBindUse { get; set; }

        public List<SKUItem> SKUItems { get; set; }

        public IEnumerable<SKUItem> GetSKUItemsNotIn(IEnumerable<Guid> skuIds)
        {
            if (skuIds == null || !skuIds.Any()) return this.SKUItems;
            return this.SKUItems.Where(s => !skuIds.Any(skuId => skuId == s.Id));
        }

        public bool Contains(Guid  skuId)
        {
            return this.SKUItems.Any(s => s.Id == skuId);
        
        }

        /// <summary>
        /// 选中一批SKUID，判断这批SKUID中是否需要其它的SKUID才能满足可用范围
        /// </summary>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public bool IsNeedOthers(IEnumerable<Guid> skuIds)
        {
            if (IsBindUse)
            {
                //至少有一个在SKUItems但是不全在才能称为NeedOthers
                bool exists = false;
                bool notExists = false;
                foreach (var skuItem in SKUItems)
                {
                    if (skuIds.Any(s => s == skuItem.Id))
                        exists = true;
                    else 
                        notExists = true;
                    if (exists && notExists)
                        return true;

                }
                return false;
            }
            else {
                foreach (var skuId in skuIds)
                {
                    if (this.SKUItems.Any(s => s.Id == skuId))
                        return false;
                }
                return true;
            }
        
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
         
            foreach (var item in SKUItems)
            {
                yield return item;
            }
        }
    }

    public class SKUItem : ValueObject
    {
        /// <summary>
        /// SKUID
        /// </summary>
        [JsonRequired]
        public Guid Id { get; set; }

        /// <summary>
        /// 好物/课程ID
        /// </summary>
        [JsonRequired]
        public Guid CourseId { get; set; }

        [JsonRequired]
        public string CourseName { get; set; }
        [JsonRequired]
        public List<PropertyItem> Properties { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }

}
