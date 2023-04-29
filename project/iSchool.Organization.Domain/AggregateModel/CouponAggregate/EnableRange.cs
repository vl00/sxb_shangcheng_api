using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    /// <summary>
    /// 可用范围基类
    /// </summary>
    public abstract class EnableRange :ValueObject
    {

        public static IEnumerable<EnableRange> Creates(string enableRangesJson)
        {

            List<EnableRange> enableRanges = new List<EnableRange>();
            if (string.IsNullOrEmpty(enableRangesJson)) return enableRanges;
            var enableRangeJarray = JArray.Parse(enableRangesJson);
            foreach (JObject item in enableRangeJarray)
            {
                if (item.TryGetValue("type", StringComparison.CurrentCultureIgnoreCase, out JToken typeToken))
                {
                    var enableRangeType = (CouponEnableRangeType)typeToken.Value<int>();
                    switch (enableRangeType)
                    {
                        case CouponEnableRangeType.SpecialGoods:
                            enableRanges.Add(item.ToObject<SKUEnableRange>());
                            break;
                        case CouponEnableRangeType.SpecialGoodsType:
                            enableRanges.Add(item.ToObject<GoodsTypeEnableRange>());
                            break;
                        case CouponEnableRangeType.SpcialBrand:
                            enableRanges.Add(item.ToObject<CourseBrandEnableRange>());
                            break;
                        case CouponEnableRangeType.Alls:
                            break;
                        default:
                            break;
                    }
                }

            }
            return enableRanges;
        }

        public static (IEnumerable<SKUEnableRange> SKUEnableRanges, IEnumerable<CourseBrandEnableRange> CourseBrandEnableRanges, IEnumerable<GoodsTypeEnableRange>  GoodsTypeEnableRanges) GetEnableRangeValues(string enableRangesJson)
        {
            List<SKUEnableRange> skuEnableRanges = new List<SKUEnableRange>();
            List<CourseBrandEnableRange> courseBrandEnableRanges = new List<CourseBrandEnableRange>();
            List<GoodsTypeEnableRange> goodsTypeEnableRanges = new List<GoodsTypeEnableRange>();
            if (string.IsNullOrEmpty(enableRangesJson)) return (skuEnableRanges, courseBrandEnableRanges, goodsTypeEnableRanges);
            var enableRangeJarray = JArray.Parse(enableRangesJson);
            foreach (JObject item in enableRangeJarray)
            {
                if (item.TryGetValue("type", StringComparison.CurrentCultureIgnoreCase, out JToken typeToken))
                {
                    var enableRangeType = (CouponEnableRangeType)typeToken.Value<int>();
                    switch (enableRangeType)
                    {
                        case CouponEnableRangeType.SpecialGoods:
                            skuEnableRanges.Add(item.ToObject<SKUEnableRange>());
                            break;
                        case CouponEnableRangeType.SpecialGoodsType:
                            goodsTypeEnableRanges.Add(item.ToObject<GoodsTypeEnableRange>());
                            break;
                        case CouponEnableRangeType.SpcialBrand:
                            courseBrandEnableRanges.Add(item.ToObject<CourseBrandEnableRange>());
                            break;
                        case CouponEnableRangeType.Alls:
                            break;
                        default:
                            break;
                    }
                }

            }
            return (skuEnableRanges, courseBrandEnableRanges, goodsTypeEnableRanges);
        }

        public static IEnumerable<EnableRange> GetEnableRangesFromJson(string enableRange_Json)
        {
            var enableRange = JArray.Parse(enableRange_Json);
            foreach (JObject item in enableRange)
            {
                if (item.TryGetValue("type", StringComparison.CurrentCultureIgnoreCase, out JToken typeToken))
                {
                    var enableRangeType = (CouponEnableRangeType)typeToken.Value<int>();
                    switch (enableRangeType)
                    {
                        case CouponEnableRangeType.SpecialGoods:
                            yield return item.ToObject<SKUEnableRange>();
                            break;
                        case CouponEnableRangeType.SpecialGoodsType:
                            yield return item.ToObject<GoodsTypeEnableRange>();
                            break;
                        case CouponEnableRangeType.SpcialBrand:
                            yield return item.ToObject<CourseBrandEnableRange>();
                            break;
                        case CouponEnableRangeType.Alls:
                            break;
                        default:
                            break;
                    }
                }


            }
        }


        /// <summary>
        /// 判断是否在可用范围内
        /// </summary>
        /// <param name="skuIds"></param>
        /// <param name="brandIds"></param>
        /// <param name="goodsTypes"></param>
        /// <param name="enableRangeJson"></param>
        /// <returns></returns>
        public static bool IsInEnableRanges(string enableRangeJson, IEnumerable<Guid> skuIds, IEnumerable<Guid> brandIds, IEnumerable<int> goodsTypes)
        {
            var enableRangeValues = EnableRange.GetEnableRangeValues(enableRangeJson);
            if ((enableRangeValues.SKUEnableRanges == null || !enableRangeValues.SKUEnableRanges.Any())
                && (enableRangeValues.CourseBrandEnableRanges == null || !enableRangeValues.CourseBrandEnableRanges.Any())
                && (enableRangeValues.GoodsTypeEnableRanges == null || !enableRangeValues.GoodsTypeEnableRanges.Any())
                )
            {
                return true;
            }

            //是否满足指定商品范围
            if (enableRangeValues.SKUEnableRanges != null && enableRangeValues.SKUEnableRanges.Any())
            {
                foreach (var item in enableRangeValues.SKUEnableRanges)
                {
                    if (!item.IsNeedOthers(skuIds))
                    {
                        return true;
                    }
                }

            }
            //是否指定品牌范围
           else if (enableRangeValues.CourseBrandEnableRanges != null && enableRangeValues.CourseBrandEnableRanges.Any())
           {
                foreach (var brandId in brandIds)
                {
                    if (enableRangeValues.CourseBrandEnableRanges.Any(s => s.Id == brandId))
                    {
                        return true;
                    }
                }
             
            }
            else if (enableRangeValues.GoodsTypeEnableRanges != null && enableRangeValues.GoodsTypeEnableRanges.Any())
            {
                foreach (var goodsType in goodsTypes)
                {
                    if (enableRangeValues.GoodsTypeEnableRanges.Any(s => goodsType == s.Id ))
                    {
                        return true;

                    }
                }
              
            }


            return false;
        }

        public CouponEnableRangeType Type { get; set; }




    }
}
