using iSchool.Organization.Appliaction.Queries;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class CouponEnableRangeJudgeCommandHandler : IRequestHandler<CouponEnableRangeJudgeCommand, IEnumerable<SKUInfo>>
    {
        IGoodsQueries _goodsQueries;
        public CouponEnableRangeJudgeCommandHandler(IGoodsQueries goodsQueries)
        {
            _goodsQueries  = goodsQueries;
        }

        public async Task<IEnumerable<SKUInfo>> Handle(CouponEnableRangeJudgeCommand request, CancellationToken cancellationToken)
        {
            var skuInfos = (await _goodsQueries.GetSKUInfosAsync(request.SKUIds)).ToList();
            if (!request.EnableRanges.Any()) return skuInfos;
            List<SKUInfo> CanUses = new List<SKUInfo>();
            foreach (var enableRange in request.EnableRanges)
            {
                if (enableRange.Type == CouponEnableRangeType.SpecialGoods)
                {
                    SKUEnableRange skuEnableRange = enableRange as SKUEnableRange;
                    if (skuEnableRange.IsBindUse)
                    {
                        //如果限制绑定使用
                        foreach (var item in skuEnableRange.SKUItems)
                        {
                            if (!skuInfos.Any(si => si.Id == item.Id))
                            {
                                continue;
                            }
                        }
                        CanUses.AddRange(skuInfos.Where(skuInfo => skuEnableRange.SKUItems.Any(si => si.Id == skuInfo.Id)));
                    }
                    else
                    {
                        foreach (var item in skuEnableRange.SKUItems)
                        {
                            var skuInfo = skuInfos.FirstOrDefault(s => s.Id == item.Id);
                            if (skuInfo != null)
                            {
                                CanUses.Add(skuInfo);
                            }
                        }

                    }
                }
                else if (enableRange.Type == CouponEnableRangeType.SpcialBrand)
                {
                    CourseBrandEnableRange courseBrandEnableRange = enableRange as CourseBrandEnableRange;
                    foreach (var skuInfo in skuInfos)
                    {
                        if (skuInfo.BrandId == courseBrandEnableRange.Id)
                        {
                            CanUses.Add(skuInfo);
                        }
                    }

                }
                else if (enableRange.Type == CouponEnableRangeType.SpecialGoodsType)
                {
                    GoodsTypeEnableRange goodsTypeEnableRange = enableRange as GoodsTypeEnableRange;
                    foreach (var skuInfo in skuInfos)
                    {
                        if (skuInfo.GoodsTypeIds.Any(g => g == goodsTypeEnableRange.Id))
                        {
                            CanUses.Add(skuInfo);
                        }
                    }
                }


            }
            return CanUses.DistinctBy(skuInfo => skuInfo.Id);
        }
    }
}
