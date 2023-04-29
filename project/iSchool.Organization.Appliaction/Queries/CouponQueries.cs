using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.Queries.Models;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CouponInfo = iSchool.Organization.Appliaction.ViewModels.Coupon.CouponInfo;
using CouponReceive = iSchool.Organization.Appliaction.ViewModels.Coupon.CouponReceive;

namespace iSchool.Organization.Appliaction.Queries
{
    public class CouponQueries : ICouponQueries
    {
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;
        public CouponQueries(IOrgUnitOfWork unitOfWork, IMediator mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            _mediator = mediator;
        }

        public async Task<CouponInfo> GetCoupon(Guid id)
        {
            var coupon = await _orgUnitOfWork.QueryFirstOrDefaultAsync<CouponInfo>("SELECT * FROM CouponInfo WHERE Id = @Id", new { Id = id });
            if (coupon == null)
            {
                throw new KeyNotFoundException($"找不到该优惠券信息。CouponId = {id}");
            }
            return coupon;

        }

        public async Task<IEnumerable<CouponInfoReceiveState>> GetShoppingCartBrandCoupons(Guid userId, ShoppingCartBrandItem brandItem,int offset = 0,int limit =20)
        {
            string sql = @"
--生成Coupon 与SKU关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A2.value, '$.Id') skuId,(case JSON_VALUE(A1.value,'$.IsBindUse') when 'true' then 1 when 'false' then 0 end) isBindUse into #skuTable  FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
OUTER APPLY OPENJSON (JSON_QUERY(A1.value,'$.SKUItems') ) A2
WHERE 
JSON_VALUE(A1.value,'$.Type') = '1'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value,'$.IsBindUse'),JSON_VALUE(A2.value, '$.Id')
--生成Coupon 与指定商品类型关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A1.value, '$.Id') goodType into #goodTypes FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
WHERE 
JSON_VALUE(A1.value,'$.Type') = '2'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value, '$.Id')
--生成Coupon 与指定品牌关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A1.value, '$.Id') brandId into #brands FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
WHERE 
JSON_VALUE(A1.value,'$.Type') = '3'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value, '$.Id')
SELECT Id into #TEMP FROM CouponInfo topcoupon {0}


SELECT  [Id],[CouponType],[Desc],[Discount]
,[Fee],[FeeOver],[ICon],[Link],[MaxFee],[MaxTake]
,[Name],[Number],[PriceOfTest],[Status],[Stock]
,[VaildDateType],[VaildEndDate],[VaildStartDate],[VaildTime]
,(case CouponType when 1 then 1 when 4 then 2 when 3 then 3 when 2 then 4 end) couponTypeWeight
,cast((Case When Exists(SELECT 1  FROM CouponInfo C OUTER APPLY OPENJSON (EnableRange_JSN)  A1  where C.Id = CouponInfo.Id and JSON_VALUE(A1.value,'$.Type') = 3) then 1 else 0 end) as bit) IsBrandCoupon
FROM CouponInfo
WHERE
[Status] = 1
AND
ISNULL(VaildEndDate,'9999-1-1') >= GETDATE()
AND 
IsHide = 0
AND 
EXISTS(SELECT 1 FROM #TEMP WHERE #TEMP.Id = CouponInfo.Id )
order by couponTypeWeight asc,PriceOfTest,Fee desc,Discount asc
offset @offset rows
fetch next @limit rows only
";
            //拼接sql 和参数
            List<string> orFilter = new List<string>() {  };
            DynamicParameters parameters = new DynamicParameters(new { offset, limit, now = DateTime.Now });
            IEnumerable<string> skuIds = brandItem.ShoppingCartSKUItems?.Select(s1 => s1.SKUId.ToString());
            List<string> brandIds = new List<string>() { brandItem.BrandId.ToString() };
            if (skuIds != null && skuIds.Any())
            {
                orFilter.Add(@"EXISTS(SELECT 1 FROM #skuTable WHERE   couponId = topcoupon.Id AND skuId  IN @skuIds)");
                parameters.Add("skuIds", skuIds);
            }
            if (brandIds != null && brandIds.Any())
            {
                orFilter.Add(@"EXISTS(SELECT 1 FROM #brands WHERE couponId = topcoupon.Id AND brandId in  @brandIds)");
                parameters.Add("brandIds", brandIds);
            }
            if (orFilter.Any())
            {
                sql = sql.FormatWith($"WHERE {string.Join(" Or ", orFilter)}");
            }

            var cpinfos = await _orgUnitOfWork.QueryAsync(sql, parameters);
            if (cpinfos.Any())
            {

                var CouponIds = cpinfos.Select<dynamic, Guid>(o => o.Id);
                IEnumerable<dynamic> CouponReceives = await _orgUnitOfWork.QueryAsync(@"SELECT [CouponId],[Status],[VaildEndTime] FROM CouponReceive  WHERE UserId = @userId and isdel = 0 And CouponId IN @CouponIds ", new { userId, CouponIds });
                var CouponReceiveSummarys = await GetCouponReceiveSummaryFromUserAsync(userId, CouponIds);
                return cpinfos.Select<dynamic, CouponInfoReceiveState>(ct => Map2CouponInfoReceiveState(ct, CouponReceives, CouponReceiveSummarys));
            }
            else
                return new List<CouponInfoReceiveState>();



        }

        public async Task<IEnumerable<CouponInfoReceiveState>> GetCouponsByBrandAsync(Guid userId, string brandId, int offset = 0, int limit = 20)
        {
            string sql = @"
--生成Coupon 与指定品牌关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A1.value, '$.Id') brandId into #brands FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
WHERE 
JSON_VALUE(A1.value,'$.Type') = '3'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value, '$.Id')
SELECT Id into #TEMP FROM CouponInfo topcoupon {0}

SELECT [Id],[CouponType],[Desc],[Discount]
,[Fee],[FeeOver],[ICon],[Link],[MaxFee],[MaxTake]
,[Name],[Number]
,[PriceOfTest]
,[Status]
,[Stock]
,[VaildDateType]
,[VaildEndDate]
,[VaildStartDate]
,[VaildTime]
,(case CouponType when 1 then 1 when 4 then 2 when 3 then 3 when 2 then 4 end) couponTypeWeight
,cast((Case When Exists(SELECT 1  FROM CouponInfo C OUTER APPLY OPENJSON (EnableRange_JSN)  A1  where C.Id = CouponInfo.Id and JSON_VALUE(A1.value,'$.Type') = 3) then 1 else 0 end) as bit) IsBrandCoupon
FROM CouponInfo
WHERE
[Status] = 1
AND 
IsHide = 0
AND 
EXISTS(SELECT 1 FROM #TEMP WHERE #TEMP.Id = CouponInfo.Id )
order by couponTypeWeight asc,PriceOfTest,Fee desc,Discount asc
offset @offset rows
fetch next @limit rows only
";
            List<dynamic> cpinfosTmp = new List<dynamic>();
            List<string> orFilter = new List<string>() ;
            //拼接sql 和参数
            DynamicParameters parameters = new DynamicParameters(new { brandId,offset, limit, now = DateTime.Now });
            orFilter.Add(@"EXISTS(SELECT 1 FROM #brands WHERE couponId = topcoupon.Id AND brandId = @brandId)");
            if (orFilter.Any())
            {
                sql = sql.FormatWith($"WHERE {string.Join(" Or ", orFilter)}");
            }
            var cpinfos = await _orgUnitOfWork.QueryAsync(sql, parameters);
            if (cpinfosTmp.Any())
            {
                var CouponIds = cpinfosTmp.Select<dynamic, Guid>(o => o.Id);
                IEnumerable<dynamic> CouponReceives = await _orgUnitOfWork.QueryAsync(@"SELECT [CouponId],[Status],[VaildEndTime] FROM CouponReceive  WHERE UserId = @userId and isdel = 0 And CouponId IN @CouponIds ", new { userId, CouponIds });
                var CouponReceiveSummarys = await GetCouponReceiveSummaryFromUserAsync(userId, CouponIds);
                return cpinfosTmp.Select<dynamic, CouponInfoReceiveState>(ct => Map2CouponInfoReceiveState(ct, CouponReceives, CouponReceiveSummarys));

            }
            else
            {
                return new List<CouponInfoReceiveState>();
            }

        }

        public async Task<IEnumerable<CouponInfoReceiveState>> GetEnableCouponsAsync(Guid userId, IEnumerable<string> skuIds, IEnumerable<string> brandIds, IEnumerable<string> goodTypes
            , int offset = 0, int limit = 20)
        {
            string sql = @"
--生成Coupon 与SKU关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A2.value, '$.Id') skuId,(case JSON_VALUE(A1.value,'$.IsBindUse') when 'true' then 1 when 'false' then 0 end) isBindUse into #skuTable  FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
OUTER APPLY OPENJSON (JSON_QUERY(A1.value,'$.SKUItems') ) A2
WHERE 
JSON_VALUE(A1.value,'$.Type') = '1'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value,'$.IsBindUse'),JSON_VALUE(A2.value, '$.Id')
--生成Coupon 与指定商品类型关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A1.value, '$.Id') goodType into #goodTypes FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
WHERE 
JSON_VALUE(A1.value,'$.Type') = '2'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value, '$.Id')
--生成Coupon 与指定品牌关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A1.value, '$.Id') brandId into #brands FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
WHERE 
JSON_VALUE(A1.value,'$.Type') = '3'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value, '$.Id')
SELECT Id into #TEMP FROM CouponInfo topcoupon {0}

SELECT [Id],[CouponType],[Desc],[Discount]
,[Fee],[FeeOver],[ICon],[Link],[MaxFee],[MaxTake]
,[Name],[Number],[PriceOfTest],[Status],[Stock]
,[VaildDateType],[VaildEndDate],[VaildStartDate],[VaildTime]
,(case CouponType when 1 then 1 when 4 then 2 when 3 then 3 when 2 then 4 end) couponTypeWeight
,cast((Case When Exists(SELECT 1  FROM CouponInfo C OUTER APPLY OPENJSON (EnableRange_JSN)  A1  where C.Id = CouponInfo.Id and JSON_VALUE(A1.value,'$.Type') = 3) then 1 else 0 end) as bit) IsBrandCoupon
FROM CouponInfo
WHERE
[Status] = 1
AND
ISNULL(VaildEndDate,'9999-1-1') >= @now
AND 
EXISTS(SELECT 1 FROM #TEMP WHERE #TEMP.Id = CouponInfo.Id )
AND 
IsHide = 0
order by couponTypeWeight asc,PriceOfTest,Fee desc,Discount asc
offset @offset rows
fetch next @limit rows only
";

            //拼接sql 和参数
            List<string> orFilter = new List<string>() { @"EnableRange_JSN = '[]'" };
            DynamicParameters parameters = new DynamicParameters(new { offset, limit, now = DateTime.Now });
            if (skuIds != null && skuIds.Any())
            {
                orFilter.Add(@"EXISTS (
SELECT 1 FROM (select groupId from #skuTable where couponId = topcoupon.Id  group by groupId) skuGroup
WHERE 
EXISTS(SELECT 1 FROM #skuTable WHERE   couponId = topcoupon.Id And groupId = skuGroup.groupId AND  isBindUse = 0 AND skuId  IN @skuIds) 
OR
(
NOT EXISTS(SELECT 1 FROM #skuTable WHERE   couponId = topcoupon.Id And groupId = skuGroup.groupId AND  isBindUse = 1  AND skuId NOT IN @skuIds)
AND
EXISTS(SELECT 1 FROM #skuTable WHERE   couponId = topcoupon.Id And groupId = skuGroup.groupId  AND skuId  IN @skuIds)
)
)");
                parameters.Add("skuIds", skuIds);
            }

            if (goodTypes != null && goodTypes.Any())
            {
                orFilter.Add(@"EXISTS(SELECT 1 FROM #goodTypes WHERE couponId = topcoupon.Id AND goodType in @goodTypes)");
                parameters.Add("goodTypes", goodTypes);
            }
            if (brandIds != null && brandIds.Any())
            {
                orFilter.Add(@"EXISTS(SELECT 1 FROM #brands WHERE couponId = topcoupon.Id AND brandId in  @brandIds)");
                parameters.Add("brandIds", brandIds);
            }
            if (orFilter.Any())
            {
                sql = sql.FormatWith($"WHERE {string.Join(" Or ", orFilter)}");
            }
            var cpinfos = await _orgUnitOfWork.QueryAsync(sql, parameters);
           

            if (cpinfos.Any())
            {
                var CouponIds = cpinfos.Select<dynamic, Guid>(o => o.Id);
                IEnumerable<dynamic> CouponReceives = await _orgUnitOfWork.QueryAsync(@"SELECT [CouponId],[Status],[VaildEndTime] FROM CouponReceive  WHERE UserId = @userId and isdel = 0 And CouponId IN @CouponIds ", new { userId, CouponIds });
                var CouponReceiveSummarys = await GetCouponReceiveSummaryFromUserAsync(userId, CouponIds);
                return cpinfos.Select<dynamic, CouponInfoReceiveState>(ct => Map2CouponInfoReceiveState(ct, CouponReceives, CouponReceiveSummarys));

            }
            else
            {
                return new List<CouponInfoReceiveState>();
            }



        }

        public async Task<IEnumerable<CouponInfoReceiveState>> GetCorrelateWithGoodsCouponsAsync(Guid userId, IEnumerable<string> skuIds, IEnumerable<string> brandIds, IEnumerable<string> goodTypes
          , int offset = 0, int limit = 20)
        {
            string sql = @"
--生成Coupon 与SKU关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A2.value, '$.Id') skuId,(case JSON_VALUE(A1.value,'$.IsBindUse') when 'true' then 1 when 'false' then 0 end) isBindUse into #skuTable  FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
OUTER APPLY OPENJSON (JSON_QUERY(A1.value,'$.SKUItems') ) A2
WHERE 
JSON_VALUE(A1.value,'$.Type') = '1'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value,'$.IsBindUse'),JSON_VALUE(A2.value, '$.Id')
--生成Coupon 与指定商品类型关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A1.value, '$.Id') goodType into #goodTypes FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
WHERE 
JSON_VALUE(A1.value,'$.Type') = '2'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value, '$.Id')
--生成Coupon 与指定品牌关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A1.value, '$.Id') brandId into #brands FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
WHERE 
JSON_VALUE(A1.value,'$.Type') = '3'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value, '$.Id')

SELECT Id into #TEMP FROM CouponInfo topcoupon {0}

SELECT [Id],[CouponType],[Desc],[Discount]
,[Fee]
,[FeeOver]
,[ICon]
,[Link]
,[MaxFee]
,[MaxTake]
,[Name]
,[Number]
,[PriceOfTest]
,[Status]
,[Stock]
,[VaildDateType]
,[VaildEndDate]
,[VaildStartDate]
,[VaildTime]
,(case CouponType when 1 then 1 when 4 then 2 when 3 then 3 when 2 then 4 end) couponTypeWeight
,cast((Case When Exists(SELECT 1  FROM CouponInfo C OUTER APPLY OPENJSON (EnableRange_JSN)  A1  where C.Id = CouponInfo.Id and JSON_VALUE(A1.value,'$.Type') = 3) then 1 else 0 end) as bit) IsBrandCoupon
FROM CouponInfo
WHERE
[Status] = 1
AND
ISNULL(VaildEndDate,'9999-1-1') >= @now
AND 
EXISTS(SELECT 1 FROM #TEMP WHERE #TEMP.Id = CouponInfo.Id )
AND 
IsHide = 0
order by couponTypeWeight asc,PriceOfTest,Fee desc,Discount asc
offset @offset rows
fetch next @limit rows only
";

            List<string> orFilter = new List<string>() { @"EnableRange_JSN = '[]'" };
            DynamicParameters parameters = new DynamicParameters() ;
            parameters.AddDynamicParams(new { offset ,limit, now = DateTime.Now });
            if (skuIds != null && skuIds.Any())
            {
                orFilter.Add(@"EXISTS(SELECT 1 FROM #skuTable WHERE   couponId = topcoupon.Id  AND skuId  IN @skuIds) ");
                parameters.Add("skuIds", skuIds);
            }

            if (goodTypes != null && goodTypes.Any())
            {
                orFilter.Add(@"EXISTS(SELECT 1 FROM #goodTypes WHERE couponId = topcoupon.Id AND goodType in @goodTypes)");
                parameters.Add("goodTypes", goodTypes);
            }
            if (brandIds != null && brandIds.Any())
            {
                orFilter.Add(@"EXISTS(SELECT 1 FROM #brands WHERE couponId = topcoupon.Id AND brandId in  @brandIds)");
                parameters.Add("brandIds", brandIds);
            }
            if (orFilter.Any())
            {
                sql = sql.FormatWith($"WHERE {string.Join(" Or ", orFilter)}");
            }
            var cpinfos = await _orgUnitOfWork.QueryAsync(sql, parameters);

            if (cpinfos.Any())
            {
                var CouponIds = cpinfos.Select<dynamic, Guid>(o => o.Id);
                IEnumerable<dynamic> CouponReceives = await _orgUnitOfWork.QueryAsync(@"SELECT [CouponId],[Status],[VaildEndTime] FROM CouponReceive  WHERE UserId = @userId and isdel = 0 And CouponId IN @CouponIds ", new { userId, CouponIds });
                var CouponReceiveSummarys = await GetCouponReceiveSummaryFromUserAsync(userId, CouponIds);
                return cpinfos.Select<dynamic, CouponInfoReceiveState>(ct => Map2CouponInfoReceiveState(ct, CouponReceives, CouponReceiveSummarys));

            }
            else
            {
                return new List<CouponInfoReceiveState>();
            }

        }

        public async Task<CouponReceiveSummary> GetCouponReceiveSummaryFromUserAsync(Guid userId, Guid couponId)
        {
            string sql = @"SELECT [UserId],[CouponId], [status],[VaildStartTime],[VaildEndTime] FROM CouponReceive  
WHERE UserId = @userId and CouponId = @couponId and isdel = 0";
            var res = await _orgUnitOfWork.QueryAsync(sql, new { userId, couponId });
            if (res.Any())
            {
                CouponReceiveSummary couponReceiveSummary = new CouponReceiveSummary(userId, couponId);
                couponReceiveSummary.TotalReceive = res.Count();
                couponReceiveSummary.UnUseCount = res.Where(res => (CouponReceiveState)res.status == CouponReceiveState.WaitUse && DateTime.Now <= res.VaildEndTime).Count();
                couponReceiveSummary.ExipreUnUseCount = res.Where(res => (CouponReceiveState)res.status == CouponReceiveState.WaitUse && DateTime.Now > res.VaildEndTime).Count();
                return couponReceiveSummary;
            }
            else
            {
                return new CouponReceiveSummary(userId, couponId);
            }
        }

        public async Task<IEnumerable<CouponReceiveSummary>> GetCouponReceiveSummaryFromUserAsync(Guid userId, IEnumerable<Guid> couponIds)
        {
            string sql = @"SELECT [UserId],[CouponId], [status],[VaildStartTime],[VaildEndTime] FROM CouponReceive  
WHERE UserId = @userId and CouponId IN @couponIds and isdel = 0";
            var res = await _orgUnitOfWork.QueryAsync(sql, new { userId, couponIds });

            if (res.Any())
            {
                return res.GroupBy(r => (Guid)r.CouponId).Select(s =>
                {

                    CouponReceiveSummary couponReceiveSummary = new CouponReceiveSummary(userId, s.Key);
                    couponReceiveSummary.TotalReceive = s.Count();
                    couponReceiveSummary.UnUseCount = s.Where(w => (CouponReceiveState)w.status == CouponReceiveState.WaitUse && DateTime.Now <= w.VaildEndTime).Count();
                    couponReceiveSummary.ExipreUnUseCount = s.Where(w => (CouponReceiveState)w.status == CouponReceiveState.WaitUse && DateTime.Now > w.VaildEndTime).Count();
                    return couponReceiveSummary;

                });
            }
            else
            {
                return null;
            }
        }


        public CouponInfoReceiveState Map2CouponInfoReceiveState(dynamic couponInfo, IEnumerable<dynamic> couponReceives, IEnumerable<CouponReceiveSummary> couponReceiveSummaries)
        {
            var couponInfoReceiveState = new CouponInfoReceiveState()
            {
                Id = couponInfo.Id,
                CouponType = (CouponType)couponInfo.CouponType,
                Desc = couponInfo.Desc,
                Discount = couponInfo.Discount,
                Fee = couponInfo.Fee,
                FeeOver = couponInfo.FeeOver,
                MaxTake = couponInfo.MaxTake,
                Name = couponInfo.Name,
                Number = CouponNumber.GetCouponNumberFromNumber((long)couponInfo.Number).ToString(),
                PriceOfTest = couponInfo.PriceOfTest,
                Stock = couponInfo.Stock,
                VaildDateType = (CouponInfoVaildDateType)couponInfo.VaildDateType,
                VaildEndDate = couponInfo.VaildEndDate,
                VaildStartDate = couponInfo.VaildStartDate,
                VaildTime = (double)couponInfo.VaildTime,
                ReceiveState = false,
                IsBrandCoupon = couponInfo.IsBrandCoupon
            };

            /*领取状态说明
             * （MaxTake > 0）
             * 领取数量 >= MaxTake = 已领取
             * 领取数量 < MaxTake 并且[有+张]处于[未过期]且([WaitUse] || [PreUse])状态 = 已领取
             * 领取数量 < MaxTake
            */
            if (couponInfoReceiveState.MaxTake > 0)
            {
                long totalReceive = couponReceiveSummaries?.FirstOrDefault(crs => crs.CouponId == couponInfoReceiveState.Id)?.TotalReceive ?? 0;
                if (totalReceive >= couponInfoReceiveState.MaxTake)
                {
                    couponInfoReceiveState.ReceiveState = true;
                }
                else
                {
                    if (couponReceives.Any(d => d.CouponId == couponInfo.Id && DateTime.Now < (DateTime)d.VaildEndTime && ((CouponReceiveState)d.Status == CouponReceiveState.WaitUse || (CouponReceiveState)d.Status == CouponReceiveState.PreUse)))
                    {
                        couponInfoReceiveState.ReceiveState = true;
                    }
                }
            }
            return couponInfoReceiveState;

        }

        public async Task<IEnumerable<CouponReceive>> GetCouponReceivesFromUserAsync(Guid userId)
        {
            string sql = @"SELECT * FROM CouponReceive  
WHERE UserId = @userId and isdel = 0";
            return await _orgUnitOfWork.QueryAsync<CouponReceive>(sql, new { userId });
        }




        public async Task<IEnumerable<CouponInfoReceiveState>> GetEnableCouponsByShoppingCartSKUItems(Guid userId, IEnumerable<ShoppingCartSKUItem> shoppingCartSKUItems, int offset = 0, int limit = 20)
        {

            var skuIds = shoppingCartSKUItems.Select(s => s.SKUId.ToString());
            var brandIds = shoppingCartSKUItems.Select(s => s.BrandId.ToString());
            var goodTypes = shoppingCartSKUItems.SelectMany(s => s.GoodsTypeIds ?? new List<int>()).Select(s => s.ToString());
            var enableCoupons = await this.GetEnableCouponsAsync(userId, skuIds, brandIds, goodTypes, offset, limit);
            List<CouponInfoReceiveState> coupons = new List<CouponInfoReceiveState>();
            foreach (var coupon in enableCoupons)
            {
                var couponInfo = Domain.AggregateModel.CouponAggregate.CouponInfo.CreateFrom(coupon.Id, coupon.CouponType, coupon.FeeOver, coupon.Fee, coupon.Discount, coupon.PriceOfTest);
                var (canBuySKUs, couponAmount, totalPrice) = couponInfo.WhatCanIUseInBuySKUsNoEnableRangeJudge(shoppingCartSKUItems.Select(s =>
                {
                    return new BuySKU()
                    {
                        BrandId = s.BrandId,
                        SKUId = s.SKUId,
                        GoodTypes = s.GoodsTypeIds,
                        Number = s.Number,
                        UnitPrice = s.UnitPrice
                    };
                }));
                if (canBuySKUs.Any())
                {
                    coupon.EstimatedAmount = couponAmount;
                    coupons.Add(coupon);
                }
            }
            return coupons.OrderByDescending(c => c.EstimatedAmount);


        }

        public async Task<IEnumerable<BrandCouDanCoupon>> GetCouDanCouponsByShoppingCartSKUItems(Guid userId, IEnumerable<ShoppingCartBrandItem> shoppingCartBrandItems)
        {

            List<BrandCouDanCoupon> brandCouDanCoupons = new List<BrandCouDanCoupon>();
            foreach (var shoppingCartBrandItem in shoppingCartBrandItems)
            {
                if (shoppingCartBrandItem.ShoppingCartSKUItems == null || !shoppingCartBrandItem.ShoppingCartSKUItems.Any()) break;
                List<string> skuIds = shoppingCartBrandItem.ShoppingCartSKUItems.Select(s => s.SKUId.ToString()).ToList();
                List<string> goodTypes = shoppingCartBrandItem.ShoppingCartSKUItems.SelectMany(s => s.GoodsTypeIds?.Select(s1 => s1.ToString()).ToList() ?? new List<string>()).ToList();
                List<string> brandIds = new List<string>() { shoppingCartBrandItem.BrandId.ToString() };
                string sql = @"
--生成Coupon 与SKU关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A2.value, '$.Id') skuId,(case JSON_VALUE(A1.value,'$.IsBindUse') when 'true' then 1 when 'false' then 0 end) isBindUse into #skuTable  FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
OUTER APPLY OPENJSON (JSON_QUERY(A1.value,'$.SKUItems') ) A2
WHERE 
JSON_VALUE(A1.value,'$.Type') = '1'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value,'$.IsBindUse'),JSON_VALUE(A2.value, '$.Id')
--生成Coupon 与指定商品类型关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A1.value, '$.Id') goodType into #goodTypes FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
WHERE 
JSON_VALUE(A1.value,'$.Type') = '2'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value, '$.Id')
--生成Coupon 与指定品牌关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A1.value, '$.Id') brandId into #brands FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
WHERE 
JSON_VALUE(A1.value,'$.Type') = '3'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value, '$.Id')

SELECT Id into #TEMP FROM CouponInfo topcoupon {0}

SELECT Top 1 [Id],[CouponType],[Desc],[Discount]
,[Fee],[FeeOver],[ICon],[Link],[MaxFee],[MaxTake]
,[Name],[Number],[PriceOfTest],[Status],[Stock]
,[VaildDateType],[VaildEndDate],[VaildStartDate],[VaildTime]
,cast((case when  EXISTS (
SELECT 1 FROM (select groupId from #skuTable where couponId = CouponInfo.Id  group by groupId) skuGroup
WHERE 
EXISTS(SELECT 1 FROM #skuTable WHERE   couponId = CouponInfo.Id And groupId = skuGroup.groupId AND  isBindUse = 1  AND skuId  IN @skuIds)
) then 1 else 0 end) as bit) IsBindUse
,(case CouponType when 2 then 1 when 3 then 2 else 0 end) CouponTypeWeight
FROM CouponInfo
WHERE
[Status] = 1
AND 
IsHide = 0
AND
(VaildEndDate IS NULL OR @now <VaildEndDate)
AND 
EXISTS(SELECT 1 FROM #TEMP WHERE #TEMP.Id = CouponInfo.Id )
order by IsBindUse DESC, CouponTypeWeight,FeeOver
";
                DynamicParameters parameters = new DynamicParameters(new { now = DateTime.Now, skuIds });
                List<string> orFilter = new List<string>() { };
                orFilter.Add(@"EXISTS (
SELECT 1 FROM (select groupId from #skuTable where couponId = topcoupon.Id  group by groupId) skuGroup
WHERE 
EXISTS(SELECT 1 FROM #skuTable WHERE   couponId = topcoupon.Id And groupId = skuGroup.groupId AND  isBindUse = 1  AND skuId  IN @skuIds)
)
");

                if (goodTypes != null && goodTypes.Any())
                {
                    orFilter.Add(@"(
EXISTS(SELECT 1 FROM #goodTypes WHERE couponId = topcoupon.Id AND goodType in @goodTypes)
AND
topcoupon.CouponType = 3
)");
                    parameters.Add("goodTypes", goodTypes);
                }
                if (brandIds != null && brandIds.Any())
                {
                    orFilter.Add(@"(
EXISTS(SELECT 1 FROM #brands WHERE couponId = topcoupon.Id AND brandId in  @brandIds)
AND
topcoupon.CouponType = 3
)");
                    parameters.Add("brandIds", brandIds);
                }
                if (orFilter.Any())
                {
                    sql = sql.FormatWith($"WHERE ({string.Join(" Or ", orFilter)})");
                }
                var res = await _orgUnitOfWork.QueryAsync(sql, parameters);
                var couponInfos = Map2CouponInfo(res);
                brandCouDanCoupons.Add(new BrandCouDanCoupon()
                {
                    BrandId = shoppingCartBrandItem.BrandId,
                    CouDanCoupons = couponInfos.Select(s => new CouDanCoupon() { IsBind = s.IsBindUse.GetValueOrDefault(), CouponInfo = s }).ToList()
                });
            }
            return brandCouDanCoupons;
        }




        public IEnumerable<CouponInfo> Map2CouponInfo(IEnumerable<dynamic> res)
        {
            foreach (var couponInfo in res)
            {
                yield return new CouponInfo()
                {
                    Id = couponInfo.Id,
                    CouponType = (CouponType)couponInfo.CouponType,
                    Desc = couponInfo.Desc,
                    Discount = couponInfo.Discount,
                    Fee = couponInfo.Fee,
                    FeeOver = couponInfo.FeeOver,
                    MaxTake = couponInfo.MaxTake,
                    Name = couponInfo.Name,
                    Number = CouponNumber.GetCouponNumberFromNumber((long)couponInfo.Number).ToString(),
                    PriceOfTest = couponInfo.PriceOfTest,
                    Stock = couponInfo.Stock,
                    VaildDateType = (CouponInfoVaildDateType)couponInfo.VaildDateType,
                    VaildEndDate = couponInfo.VaildEndDate,
                    VaildStartDate = couponInfo.VaildStartDate,
                    VaildTime = (double)couponInfo.VaildTime,
                    IsBindUse = couponInfo.IsBindUse,
                    EnableRange_JSN = couponInfo.EnableRange_JSN
                };
            }


        }

        public async Task<(IEnumerable<CouponReceive> data, int total)> GetMyWaitUseCouponsAsync(Guid userId, int offset = 0, int limit = 20)
        {
            string sql = @"SELECT CouponReceive.Id
,CouponReceive.GetTime
,CouponReceive.Number
,CouponReceive.OriginType
,CouponReceive.ReadTime
,CouponReceive.Remark
,CouponReceive.Status
,CouponReceive.UsedTime
,CouponReceive.UserId
,CouponReceive.VaildEndTime
,CouponReceive.VaildStartTime
,CouponInfo.Id CouponId
,CouponInfo.Number CouponNumber
,CouponInfo.[Name] 
,CouponInfo.[Desc]
,CouponInfo.VaildDateType
,CouponInfo.VaildStartDate
,CouponInfo.VaildEndDate
,CouponInfo.VaildTime
,CouponInfo.CouponType
,CouponInfo.Fee
,CouponInfo.FeeOver
,CouponInfo.Discount
,CouponInfo.PriceOfTest
,cast((Case When Exists(SELECT 1  FROM CouponInfo C OUTER APPLY OPENJSON (EnableRange_JSN)  A1  where C.Id = CouponInfo.Id and JSON_VALUE(A1.value,'$.Type') = 3) then 1 else 0 end) as bit) IsBrandCoupon
FROM CouponReceive
JOIN CouponInfo ON CouponInfo.Id = CouponReceive.CouponId
WHERE
USERID = @userId
AND ( CouponReceive.VaildEndTime > GETDATE() AND CouponReceive.[Status] = 1)
AND CouponReceive.IsDel = 0
ORDER BY GetTime DESC
OFFSET @offset ROWS
FETCH NEXT @limit ROWS ONLY;
SELECT 
Count(1)
FROM CouponReceive
JOIN CouponInfo ON CouponInfo.Id = CouponReceive.CouponId
WHERE
USERID = @userId
AND ( CouponReceive.VaildEndTime > GETDATE() AND CouponReceive.[Status] = 1)
AND CouponReceive.IsDel = 0
";
            using (var grid = await _orgUnitOfWork.QueryMultipleAsync(sql, new { userId, offset, limit }, _orgUnitOfWork.DbTransaction))
            {
                var res = await grid.ReadAsync();
                var couponReceives = MapCouponReceive(res);
                var total = await grid.ReadFirstAsync<int>();


                return (couponReceives.OrderByDescending(s => s.FlagState).ThenByDescending(s => s.GetTime), total);

            }

        }




        public async Task<IEnumerable<CouponReceive>> GetMyLoseEfficacyCouponsAsync(Guid userId, int offset = 0, int limit = 20)
        {
            string sql = @"SELECT CouponReceive.Id
,CouponReceive.GetTime
,CouponReceive.Number
,CouponReceive.OriginType
,CouponReceive.ReadTime
,CouponReceive.Remark
,CouponReceive.Status
,CouponReceive.UsedTime
,CouponReceive.UserId
,CouponReceive.VaildEndTime
,CouponReceive.VaildStartTime
,CouponInfo.Id CouponId
,CouponInfo.Number CouponNumber
,CouponInfo.[Name] 
,CouponInfo.[Desc]
,CouponInfo.VaildDateType
,CouponInfo.VaildStartDate
,CouponInfo.VaildEndDate
,CouponInfo.VaildTime
,CouponInfo.CouponType
,CouponInfo.Fee
,CouponInfo.FeeOver
,CouponInfo.Discount
,CouponInfo.PriceOfTest
,cast((Case When Exists(SELECT 1  FROM CouponInfo C OUTER APPLY OPENJSON (EnableRange_JSN)  A1  where C.Id = CouponInfo.Id and JSON_VALUE(A1.value,'$.Type') = 3) then 1 else 0 end) as bit) IsBrandCoupon
FROM CouponReceive
JOIN CouponInfo ON CouponInfo.Id = CouponReceive.CouponId
WHERE
USERID = @userId
AND ( CouponReceive.VaildEndTime < GETDATE() Or CouponReceive.[Status] != 1)
AND CouponReceive.IsDel = 0
ORDER BY  UsedTime Desc,GetTime DESC
OFFSET @offset ROWS
FETCH NEXT @limit ROWS ONLY";


            var res = await _orgUnitOfWork.QueryAsync(sql, new { userId, offset, limit });
            var couponReceives = MapCouponReceive(res);
            return couponReceives;
        }

        IEnumerable<CouponReceive> MapCouponReceive(IEnumerable<dynamic> res)
        {
            foreach (var item in res)
            {
                CouponReceive couponReceive = new CouponReceive()
                {
                    Id = item.Id,
                    CouponId = item.CouponId,
                    CouponNumber = CouponNumber.GetCouponNumberFromNumber((long)item.CouponNumber).ToString(),
                    CouponType = (CouponType)item.CouponType,
                    Desc = item.Desc,
                    Discount = item.Discount,
                    Fee = item.Fee,
                    FeeOver = item.FeeOver,
                    GetTime = item.GetTime,
                    Name = item.Name,
                    Number = CouponNumber.GetCouponNumberFromNumber((long)item.Number).ToString(),
                    PriceOfTest = item.PriceOfTest,
                    Remark = item.Remark,
                    Status = (CouponReceiveState)item.Status,
                    UsedTime = item.UsedTime,
                    UserId = item.UserId,
                    VaildEndTime = item.VaildEndTime,
                    VaildStartTime = item.VaildStartTime,
                    VaildDateType = (CouponInfoVaildDateType)item.VaildDateType,
                    EnableRange_JSN = item.EnableRange_JSN,
                    IsBrandCoupon = item.IsBrandCoupon
                };
                yield return couponReceive;
            }
        }


        public async Task<PaginationResult<CouponReceiveSummariesResponse>> GetCouponReceivePageAsync(CouponReceiveQueryModel queryModel)
        {
            if (queryModel.Status == CouponReceiveStateExt.Expire)
            {
                queryModel.Status = null;
                queryModel.EndDate = DateTime.Now;
            }
            var searchUserIds = Enumerable.Empty<Guid>();

            if (!string.IsNullOrWhiteSpace(queryModel.Phone)
                || !string.IsNullOrWhiteSpace(queryModel.NickName))
            {
                var searchUsers = await _mediator.Send(new UserInfoByNameOrMobileQuery
                {
                    Mobile = queryModel.Phone,
                    Name = queryModel.NickName,
                });
                if (!searchUsers.Any())
                {
                    return PaginationResult<CouponReceiveSummariesResponse>.Default(queryModel.PageIndex, queryModel.PageSize);
                }

                searchUserIds = searchUsers.Select(s => s.Id);
            }
            
            queryModel.ReceiveNumber = string.IsNullOrWhiteSpace(queryModel.ReceiveNumber) ? null : queryModel.ReceiveNumber;
            queryModel.CouponNumber = string.IsNullOrWhiteSpace(queryModel.CouponNumber) ? null : queryModel.CouponNumber;
            queryModel.CouponName = string.IsNullOrWhiteSpace(queryModel.CouponName) ? null :  $"%{queryModel.CouponName}%";
            queryModel.EnableRange = string.IsNullOrWhiteSpace(queryModel.EnableRange) ? null :  $"%{queryModel.EnableRange}%";
            var userSql = searchUserIds.Any() ? " AND CR.UserId in @userIds " : "";
            string sql = $@"
SELECT 
	CR.*
FROM 
	CouponReceive CR
WHERE
	IsDel = 0
	{userSql}
	AND (@status is null or CR.Status = @status)
	AND (@receiveNumber is null or CR.Number = @receiveNumber)
	AND (@startDate is null or CR.VaildStartTime >= @startDate)
	AND (@endDate is null or CR.VaildEndTime <= @endDate)
	AND EXISTS (
		SELECT 1 FROM CouponInfo CI
		WHERE CI.Id = CR.CouponId
			AND (@couponName is null or CI.Name Like @couponName)
			AND (@couponNumber is null or CI.Number = @couponNumber)
			AND (@couponType is null or CI.CouponType = @couponType)
			AND (@enableRange is null or CI.EnableRange_JSN Like @enableRange)
	)
order by CR.GetTime desc
offset (@pageIndex-1)*@pageSize rows 
fetch next @pageSize row only "
;

            var couponReceives = await _orgUnitOfWork.QueryAsync<CouponReceive>(sql, new
            {
                userIds = searchUserIds,
                status = queryModel.Status,
                receiveNumber = queryModel.ReceiveNumber,
                couponName = queryModel.CouponName,
                couponNumber = queryModel.CouponNumber,
                couponType = queryModel.CouponType,
                enableRange = queryModel.EnableRange,
                startDate = queryModel.StartDate,
                endDate = queryModel.EndDate,
                pageIndex = queryModel.PageIndex,
                pageSize = queryModel.PageSize
            });

            var couponIds = couponReceives.Select(s => s.CouponId);
            //券
            var dynamicCoupons = await _orgUnitOfWork.QueryAsync(@"SELECT [Id]
      ,[Number]
      ,[Name]
      ,[Desc]
      ,[VaildDateType]
      ,[VaildStartDate]
      ,[VaildEndDate]
      ,[VaildTime]
      ,[MaxTake]
      ,[Stock]
      ,[Total]
      ,[CouponType]
      ,[Fee]
      ,[FeeOver]
      ,[Discount]
      ,[MaxFee]
      ,[PriceOfTest]
      ,[GetStartTime]
      ,[GetEndTime]
      ,[Remark]
      ,[Status]
      ,[CanMultiple]
      ,[Link]
      ,[CreateTime]
      ,[Creator]
      ,[CanBack]
      ,[ICon]
      ,[IsHide]
      ,[KeyWord]
      ,[Updator]
      ,[UpdateTime] FROM CouponInfo WHERE Id in @couponIds", new { couponIds });
            var coupons = Map2CouponInfo(dynamicCoupons);
           var couponsEnableRangeSummaries =  await GetEnableRangeSummarys(coupons.Select(s => s.Id));


            //用户
            var users = await _mediator.Send(new UserInfosByAPICommand() { UserIds = couponReceives.Select(s => s.UserId).Distinct() });


            var result = couponReceives.Select(s =>
            {
                var user = users.FirstOrDefault(user => user.Id == s.UserId);
                var coupon = coupons.FirstOrDefault(coupon => coupon.Id == s.CouponId) ?? new CouponInfo();

                var formatReceiveNumber = s.Number;
                if (long.TryParse(s.Number, out long number))
                {
                    formatReceiveNumber = CouponNumber.GetCouponNumberFromNumber(number).ToString();
                }
                return new CouponReceiveSummariesResponse()
                {
                    Id = s.Id,
                    OrderId = s.OrderId,
                    UserId = s.UserId,
                    NickName = user?.NickName,
                    Phone = user?.Mobile,
                    CouponId = s.CouponId,
                    CouponName = coupon.Name,
                    CouponNumber = coupon.Number,
                    CouponValue = CouponValue.GetCouponValue(coupon.CouponType, coupon.Fee, coupon.FeeOver, coupon.PriceOfTest, coupon.Discount),
                    CouponType = coupon.CouponType,
                    CouponDesc = coupon.Desc,
                    EnableRangeSummaries = couponsEnableRangeSummaries.FirstOrDefault(s1=>s1.CouponId == s.CouponId).EnableRangeSummaries,
                    Status = s.StatusExt,
                    ReceiveNumber = formatReceiveNumber,
                    ExpireDateString = $"{s.VaildStartTime:yyyy-MM-dd}至{s.VaildEndTime:yyyy-MM-dd}" //CouponValue.GetCouponExpireDate(coupon.VaildDateType,),
                };
            });

            return new PaginationResult<CouponReceiveSummariesResponse>()
            {
                Data = result,
                Total = await GetCouponReceiveTotalAsync(queryModel, searchUserIds),
                PageIndex = queryModel.PageIndex,
                PageSize = queryModel.PageSize,
            };
        }





        public async Task<CouponReceiveDetailResponse> GetCouponReceiveDetailAsync(Guid id)
        {


            string sql = $@"
SELECT 
	CR.*
FROM 
	CouponReceive CR
WHERE
	IsDel = 0 and id = @id "
;
            var couponReceives = await _orgUnitOfWork.QueryAsync<CouponReceive>(sql, new{id});

            var couponIds = couponReceives.Select(s => s.CouponId);
            //券
            var dynamicCoupons = await _orgUnitOfWork.QueryAsync(@"SELECT * FROM CouponInfo WHERE Id in @couponIds", new { couponIds });
            var coupons = Map2CouponInfo(dynamicCoupons);

            //用户
            var users = await _mediator.Send(new UserInfosByAPICommand() { UserIds = couponReceives.Select(s => s.UserId).Distinct() });


            var result = couponReceives.Select(s =>
            {
                var user = users.FirstOrDefault(user => user.Id == s.UserId);
                var coupon = coupons.FirstOrDefault(coupon => coupon.Id == s.CouponId) ?? new CouponInfo();

                var formatReceiveNumber = s.Number;
                if (long.TryParse(s.Number, out long number))
                {
                    formatReceiveNumber = CouponNumber.GetCouponNumberFromNumber(number).ToString();
                }
                return new CouponReceiveDetailResponse()
                {
                    Id = s.Id,
                    OrderId = s.OrderId,
                    UserId = s.UserId,
                    NickName = user?.NickName,
                    Phone = user?.Mobile,
                    CouponId = s.CouponId,
                    CouponName = coupon.Name,
                    CouponNumber = coupon.Number,
                    CouponValue = CouponValue.GetCouponValue(coupon.CouponType, coupon.Fee, coupon.FeeOver, coupon.PriceOfTest, coupon.Discount),
                    CouponType = coupon.CouponType,
                    CouponDesc = coupon.Desc,
                    EnableRange = EnableRange.GetEnableRangesFromJson(coupon.EnableRange_JSN),
                    Status = s.StatusExt,
                    ReceiveNumber = formatReceiveNumber,
                    ExpireDateString = $"{s.VaildStartTime:yyyy-MM-dd}至{s.VaildEndTime:yyyy-MM-dd}" //CouponValue.GetCouponExpireDate(coupon.VaildDateType,),
                };
            });

            return result.FirstOrDefault();
        }



        public async Task<long> GetCouponReceiveTotalAsync(CouponReceiveQueryModel queryModel, IEnumerable<Guid> searchUserIds)
        {
            //cache

            var userSql = searchUserIds.Any() ? " AND CR.UserId in @userIds " : "";
            string sql = $@"
SELECT 
	count(1)
FROM 
	CouponReceive CR
WHERE
	IsDel = 0
	{userSql}
	AND (@status is null or CR.Status = @status)
	AND (@receiveNumber is null or CR.Number = @receiveNumber)
	AND (@startDate is null or CR.VaildStartTime <= @startDate)
	AND (@endDate is null or CR.VaildEndTime >= @endDate)
	AND EXISTS (
		SELECT 1 FROM CouponInfo CI
		WHERE CI.Id = CR.CouponId
			AND (@couponName is null or CI.Name Like @couponName)
			AND (@couponNumber is null or CI.Number = @couponNumber)
			AND (@couponType is null or CI.CouponType = @couponType)
			AND (@enableRange is null or CI.EnableRange_JSN Like @enableRange)
	)
";

            var total = await _orgUnitOfWork.QueryFirstOrDefaultAsync<long>(sql, new
            {
                userIds = searchUserIds,
                status = queryModel.Status,
                receiveNumber = queryModel.ReceiveNumber,
                couponName = queryModel.CouponName,
                couponNumber = queryModel.CouponNumber,
                couponType = queryModel.CouponType,
                enableRange = queryModel.EnableRange,
                startDate = queryModel.StartDate,
                endDate = queryModel.EndDate,
                pageIndex = queryModel.PageIndex,
                pageSize = queryModel.PageSize
            });

            return total;
        }

        public async Task<IEnumerable<CouponReceive>> GetMyCouponsByCashierSKUItems(Guid userId, IEnumerable<ShoppingCartSKUItem> shoppingCartSKUItems)
        {
            string sql = @"
--生成Coupon 与SKU关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A2.value, '$.Id') skuId,(case JSON_VALUE(A1.value,'$.IsBindUse') when 'true' then 1 when 'false' then 0 end) isBindUse into #skuTable  FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
OUTER APPLY OPENJSON (JSON_QUERY(A1.value,'$.SKUItems') ) A2
WHERE 
JSON_VALUE(A1.value,'$.Type') = '1'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value,'$.IsBindUse'),JSON_VALUE(A2.value, '$.Id')
--生成Coupon 与指定商品类型关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A1.value, '$.Id') goodType into #goodTypes FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
WHERE 
JSON_VALUE(A1.value,'$.Type') = '2'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value, '$.Id')
--生成Coupon 与指定品牌关系临时表
SELECT A1.[Key] groupId , Id couponId ,JSON_VALUE(A1.value, '$.Id') brandId into #brands FROM CouponInfo
OUTER APPLY OPENJSON (EnableRange_JSN)  A1 
WHERE 
JSON_VALUE(A1.value,'$.Type') = '3'  
GROUP BY Id,A1.[Key],JSON_VALUE(A1.value, '$.Id')

SELECT Id into #TEMP FROM CouponInfo topcoupon {0}
  

----分割---
SELECT CouponReceive.Id
,CouponReceive.GetTime
,CouponReceive.Number
,CouponReceive.OriginType
,CouponReceive.ReadTime
,CouponReceive.Remark
,CouponReceive.Status
,CouponReceive.UsedTime
,CouponReceive.UserId
,CouponReceive.VaildEndTime
,CouponReceive.VaildStartTime
,CouponInfo.Id CouponId
,CouponInfo.Number CouponNumber
,CouponInfo.[Name] 
,CouponInfo.[Desc]
,CouponInfo.VaildDateType
,CouponInfo.VaildStartDate
,CouponInfo.VaildEndDate
,CouponInfo.VaildTime
,CouponInfo.CouponType
,CouponInfo.Fee
,CouponInfo.FeeOver
,CouponInfo.Discount
,CouponInfo.PriceOfTest
,cast((Case When Exists(SELECT 1  FROM CouponInfo C OUTER APPLY OPENJSON (EnableRange_JSN)  A1  where C.Id = CouponInfo.Id and JSON_VALUE(A1.value,'$.Type') = 3) then 1 else 0 end) as bit) IsBrandCoupon
FROM CouponReceive
JOIN CouponInfo ON CouponReceive.CouponId = CouponInfo.Id
WHERE
CouponReceive.[Status] = 1
AND
CouponReceive.IsDel = 0
AND
ISNULL(CouponInfo.VaildEndDate,'9999-1-1') >= @now
AND
@now BETWEEN CouponReceive.VaildStartTime AND CouponReceive.VaildEndTime
AND 
EXISTS(SELECT 1 FROM #TEMP WHERE #TEMP.Id = CouponInfo.Id )
AND
CouponReceive.UserId = @userId
";

            var skuIds = shoppingCartSKUItems.Select(s => s.SKUId.ToString());
            var brandIds = shoppingCartSKUItems.Select(s => s.BrandId.ToString());
            var goodTypes = shoppingCartSKUItems.SelectMany(s => s.GoodsTypeIds ?? new List<int>()).Select(s => s.ToString());

            //拼接sql 和参数
            List<string> orFilter = new List<string>() { @"EnableRange_JSN = '[]'" };
            DynamicParameters parameters = new DynamicParameters(new { now = DateTime.Now});
            parameters.Add("userId", userId);
            if (skuIds != null && skuIds.Any())
            {
                orFilter.Add(@"EXISTS (
SELECT 1 FROM (select groupId from #skuTable where couponId = topcoupon.Id  group by groupId) skuGroup
WHERE 
EXISTS(SELECT 1 FROM #skuTable WHERE   couponId = topcoupon.Id And groupId = skuGroup.groupId AND  isBindUse = 0 AND skuId  IN @skuIds) 
OR
(
NOT EXISTS(SELECT 1 FROM #skuTable WHERE   couponId = topcoupon.Id And groupId = skuGroup.groupId AND  isBindUse = 1  AND skuId NOT IN @skuIds)
AND
EXISTS(SELECT 1 FROM #skuTable WHERE   couponId = topcoupon.Id And groupId = skuGroup.groupId  AND skuId  IN @skuIds)
)
)");
                parameters.Add("skuIds", skuIds);
            }

            if (goodTypes != null && goodTypes.Any())
            {
                orFilter.Add(@"EXISTS(SELECT 1 FROM #goodTypes WHERE couponId = topcoupon.Id AND goodType in @goodTypes)");
                parameters.Add("goodTypes", goodTypes);
            }
            if (brandIds != null && brandIds.Any())
            {
                orFilter.Add(@"EXISTS(SELECT 1 FROM #brands WHERE couponId = topcoupon.Id AND brandId in  @brandIds)");
                parameters.Add("brandIds", brandIds);
            }
            if (orFilter.Any())
            {
                sql = sql.FormatWith($"WHERE {string.Join(" Or ", orFilter)}");
            }
            var res = await _orgUnitOfWork.QueryAsync(sql, parameters);
            var couponReceives = MapCouponReceive(res).ToList();
            List<CouponReceive> enableCouponReceives = new List<CouponReceive>();
            foreach (var couponReceive in couponReceives)
            {
                var couponInfo = Domain.AggregateModel.CouponAggregate.CouponInfo.CreateFrom(couponReceive.CouponId, couponReceive.CouponType, couponReceive.FeeOver, couponReceive.Fee, couponReceive.Discount, couponReceive.PriceOfTest);
                var (canBuySKUs, couponAmount, totalPrice) = couponInfo.WhatCanIUseInBuySKUsNoEnableRangeJudge(shoppingCartSKUItems.Select(s => new BuySKU()
                {
                    BrandId = s.BrandId,
                    SKUId = s.SKUId,
                    GoodTypes = s.GoodsTypeIds,
                    Number = s.Number,
                    UnitPrice = s.UnitPrice
                }));
                if (canBuySKUs.Any())
                {
                    //计算预估金额
                    couponReceive.EstimatedAmount = couponAmount;
                    enableCouponReceives.Add(couponReceive);
                }
            }

            return enableCouponReceives.OrderByDescending(c => c.EstimatedAmount);
        }

        public async Task<IEnumerable<Guid>> GetCouponBindUseSKUIds(Guid couponId, IEnumerable<Guid> notInSkuIds)
        {
            List<Guid> skuIds = new List<Guid>();
            string sql = @"SELECT EnableRange_JSN FROM CouponInfo WHERE Id = @id";
            string enableRangeJson = await _orgUnitOfWork.ExecuteScalarAsync<string>(sql, new { id = couponId });
            var enableRanges = EnableRange.Creates(enableRangeJson);
            foreach (var enableRange in enableRanges)
            {
                if (enableRange is SKUEnableRange)
                {
                   var skuEnableRange =  enableRange as SKUEnableRange;
                    if (skuEnableRange.IsBindUse)
                    {
                        skuIds.AddRange(skuEnableRange.GetSKUItemsNotIn(notInSkuIds).Select(s => s.Id));
                    }
                }
            }

            return skuIds;

        }
        public async Task<(IEnumerable<Guid> SKUIds, IEnumerable<Guid> BrandIds,IEnumerable<int> GoodTypes)> GetCouponEnableRangeValue(Guid couponId,IEnumerable<Guid> notInSkuIdsWhenIsBind = null)
        {
            string sql = @"SELECT EnableRange_JSN FROM CouponInfo WHERE Id = @id";
            string enableRangeJson = await _orgUnitOfWork.ExecuteScalarAsync<string>(sql, new { id = couponId });
            var enableRange = EnableRange.GetEnableRangeValues(enableRangeJson);
            IEnumerable<Guid> skuIds = enableRange.SKUEnableRanges.SelectMany(s=> {

                List<Guid> skuIds = new List<Guid>();
                if (s.IsBindUse && (notInSkuIdsWhenIsBind!= null && notInSkuIdsWhenIsBind.Any()))
                {
                    foreach (var item in s.SKUItems)
                    {
                        if (!notInSkuIdsWhenIsBind.Any(s1 => s1 == item.Id))
                        {
                            skuIds.Add(item.Id);
                        }

                    }
                }
                else {
                    skuIds.AddRange(s.SKUItems.Select(s1 => s1.Id));
                }
                return skuIds;
            });
            IEnumerable<Guid> brandIds = enableRange.CourseBrandEnableRanges.Select(s=>s.Id);
            IEnumerable<int> goodTypes = enableRange.GoodsTypeEnableRanges.Select(s=>s.Id);
            return (skuIds, brandIds, goodTypes);

        }


        public async Task<IEnumerable<OrderUseCouponInfo>> GetOrderUseCouponInfosAsync(string? advanceOrderNo, Guid? orderId)
        {
            if (advanceOrderNo.IsNullOrEmpty() && orderId == null) throw new ArgumentNullException("advanceOrderNo 和 orderId 必须传入其中一个。");
            string sql = @"
SELECT O.id OrderId,CouponInfo.[Name] CouponName,SUM(OrderDiscount.DiscountAmount) CouponAmount,TotalPayment FROM CouponInfo
JOIN CouponReceive ON CouponReceive.CouponId = CouponInfo.Id
JOIN [Order] O ON  O.AdvanceOrderId = CouponReceive.OrderId
JOIN OrderDetial ON OrderDetial.orderid = O.id
JOIN OrderDiscount ON OrderDiscount.OrderId = OrderDetial.id
WHERE {0}
GROUP BY O.id,O.totalpayment,CouponInfo.[Name]";
            DynamicParameters parameters = new DynamicParameters();
            if (!advanceOrderNo.IsNullOrEmpty())
            {
                sql = sql.FormatWith("O.AdvanceOrderNo = @advanceOrderNo");
                parameters.Add("advanceOrderNo", advanceOrderNo);
            }
            else if (orderId != null)
            {
                sql = sql.FormatWith("O.id = @orderId");
                parameters.Add("orderId", orderId);
            }
            var res = await _orgUnitOfWork.QueryAsync<OrderUseCouponInfo>(sql, parameters, _orgUnitOfWork.DbTransaction);
            return res;
        }

        public async Task<IEnumerable<CouponReceive>> GetWillExpireCouponReceives()
        {
            string sql = @"SELECT * FROM CouponReceive
WHERE 
[Status] = 1 AND WillExpireMessageNotify = 0 and IsDel = 0
AND 
CAST(GETDATE() AS date) = CAST(DATEADD(DAY,-1,VaildEndTime) as date)";
            return await _orgUnitOfWork.QueryAsync<CouponReceive>(sql, _orgUnitOfWork.DbTransaction);
        }

        public async Task<IEnumerable<(Guid CouponId, IEnumerable<EnableRangeSummary> EnableRangeSummaries)>> GetEnableRangeSummarys(IEnumerable<Guid> couponIds)
        {
            string sql = @"SELECT 
Id CouponId
,A1.[Key] GroupId
,Cast(JSON_Value(A1.value,'$.Type') as int) EnableRangeType
,(case JSON_Value(A1.value,'$.Type') 
when 1  then ('{""FirstItemId"":""'+JSON_Value(A1.value,'$.SKUItems[0].Id')+'"",""FirstItemName"":""'+JSON_Value(A1.value,'$.SKUItems[0].CourseName')+'"",""ItemTotals"":'+cast((SELECT COUNT(1) FROM OpenJSON(JSON_Query(A1.value,'$.SKUItems'))) as varchar)+'}')
when 2  then('{""Id"":' + JSON_Value(A1.value, '$.Id') + ',""Name"":""' + JSON_Value(A1.value, '$.Name') + '""}')
when 3  then('{""Id"":""' + JSON_Value(A1.value, '$.Id') + '"",""Name"":""' + JSON_Value(A1.value, '$.Name') + '""}')
end) RangeSummary
FROM CouponInfo
OUTER APPLY OPENJSON(EnableRange_JSN)  A1
WHERE CouponInfo.Id IN @CouponIds";
          var res =  await _orgUnitOfWork.QueryAsync(sql, new { CouponIds = couponIds });
            return MapEnableRangeSummaries(res);
        }


        IEnumerable<(Guid CouponId, IEnumerable<EnableRangeSummary> EnableRangeSummaries)> MapEnableRangeSummaries(IEnumerable<dynamic> res)
        {
            List<(Guid CouponId, IEnumerable<EnableRangeSummary> EnableRangeSummaries)> result = new List<(Guid CouponId, IEnumerable<EnableRangeSummary> EnableRangeSummaries)>();
            if (res.Any())
            {
                foreach (var groupItem in res.GroupBy(s=>s.CouponId))
                {
                    List<EnableRangeSummary> enableRangeSummaries = new List<EnableRangeSummary>();
                    foreach (var item in groupItem) 
                    {
                        EnableRangeSummary summary = null;
                        if (item.EnableRangeType == null) continue;
                        if((CouponEnableRangeType)item.EnableRangeType == CouponEnableRangeType.SpecialGoods)
                            summary =  JsonConvert.DeserializeObject<SKURangeSummary>(item.RangeSummary);
                        if ((CouponEnableRangeType)item.EnableRangeType == CouponEnableRangeType.SpcialBrand)
                            summary = JsonConvert.DeserializeObject<BrandRangeSummary>(item.RangeSummary);
                        if ((CouponEnableRangeType)item.EnableRangeType == CouponEnableRangeType.SpecialGoodsType)
                            summary = JsonConvert.DeserializeObject<GoodTypeRangeSummary>(item.RangeSummary);
                        summary.EnableRangeType = (CouponEnableRangeType)item.EnableRangeType;
                        enableRangeSummaries.Add(summary);


                    }
                    result.Add((groupItem.Key, enableRangeSummaries));
                }            
            }

            return result;
            
        }

        public async Task<CouponInfoItem> GetCouponInfoItem(Guid id)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.AddDynamicParams(new { id });
            string sql = @"
  SELECT 
      [Id]
      ,[Number]
      ,[Name]
      ,[Desc]
      ,[VaildDateType]
      ,[VaildStartDate]
      ,[VaildEndDate]
      ,[VaildTime]
      ,[MaxTake]
      ,[Stock]
      ,[Total]
      ,[CouponType]
      ,[Fee]
      ,[FeeOver]
      ,[Discount]
      ,[MaxFee]
      ,[PriceOfTest]
      ,[GetStartTime]
      ,[GetEndTime]
      ,[Remark]
      ,[Status]
      ,[CanMultiple]
      ,[Link]
      ,[CreateTime]
      ,[Creator]
      ,[CanBack]
      ,[ICon]
      ,[IsHide]
      ,[EnableRange_JSN]
      ,[KeyWord]
      ,[Updator]
      ,[UpdateTime]
  FROM CouponInfo
  WHERE Id = @id
";
            var res = await _orgUnitOfWork.QueryFirstAsync(sql, parameters);
            var couponInfoItem = MapCouponInfoItem(res);
            return couponInfoItem;
        }


        CouponInfoItem MapCouponInfoItem(dynamic couponInfo)
        {
            return new CouponInfoItem()
            {
                Id = couponInfo.Id,
                MaxTake = couponInfo.MaxTake,
                Title = couponInfo.Name,
                Number = CouponNumber.GetCouponNumberFromNumber((long)couponInfo.Number).ToString(),
                State = new CouponStateValue((CouponInfoState)couponInfo.Status, couponInfo.IsHide).CouponState,
                Total = couponInfo.Total,
                Stock = couponInfo.Stock,
                Type =(CouponType)couponInfo.CouponType,
                Value = CouponValue.GetCouponValue((CouponType)couponInfo.CouponType, couponInfo.Fee, couponInfo.FeeOver, couponInfo.PriceOfTest, couponInfo.Discount),
                RuleDesc = couponInfo.Desc,
                ValidTime =new CouponVaildTimeFormatter((CouponInfoVaildDateType)couponInfo.VaildDateType, couponInfo.VaildStartDate, couponInfo.VaildEndDate, (long)couponInfo.VaildTime).BGListFormatt(),
                EnableRange = EnableRange.Creates(couponInfo.EnableRange_JSN),
                Discount = couponInfo.Discount,
                Fee = couponInfo.Fee,
                FeeOver = couponInfo.FeeOver,
                PriceOfTest = couponInfo.PriceOfTest,
                VaildDateType = (CouponInfoVaildDateType)couponInfo.VaildDateType,
                VaildDay = (int)TimeSpan.FromHours((double)couponInfo.VaildTime).TotalDays,
                VaildEndDate = couponInfo.VaildEndDate,
                VaildStartDate = couponInfo.VaildStartDate
            };
        }


    }


}
