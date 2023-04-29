using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.Queries;
using iSchool.Organization.Appliaction.RequestModels.Coupon;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CouponInfo = iSchool.Organization.Domain.AggregateModel.CouponAggregate.CouponInfo;

namespace iSchool.Organization.Appliaction.Service.Coupon
{
    public class CouponInfosFilterQueryHandler : IRequestHandler<CouponInfosFilterQuery, CouponInfos>
    {

        OrgUnitOfWork _orgUnitOfWork;
        ICouponQueries _couponQueries;
        public CouponInfosFilterQueryHandler(IOrgUnitOfWork orgUnitOfWork
            , ICouponQueries couponQueries)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            _couponQueries = couponQueries;
        }

        public async Task<CouponInfos> Handle(CouponInfosFilterQuery request, CancellationToken cancellationToken)
        {
            DynamicParameters parameters = new DynamicParameters();
            int offset = (request.Page - 1) * request.PageSize;
            int limit = request.PageSize;
            parameters.AddDynamicParams(new { offset, limit });
            string filter;

            if (!string.IsNullOrEmpty(request.Num) && int.TryParse(request.Num, out int number))
            {
                filter = " Number = @Number ";
                parameters.Add("Number", number);

            }
            else
            {
                List<string> andfilters = new List<string>();
                if (request.Type != null)
                {
                    andfilters.Add(" CouponType = @CouponType ");
                    parameters.Add("CouponType", request.Type);
                }
                if (request.State != null)
                {
                    if (request.State == CouponState.Online)
                    {
                        andfilters.Add(" Status = @Status ");
                        parameters.Add("Status", CouponInfoState.Online);
                    }
                    else if (request.State == CouponState.Offline)
                    {
                        andfilters.Add(" Status = @Status ");
                        parameters.Add("Status", CouponInfoState.Offline);
                    }
                    else if (request.State == CouponState.LoseEfficacy)
                    {
                        andfilters.Add(" Status = @Status ");
                        parameters.Add("Status", CouponInfoState.LoseEfficacy);
                    }
                    else if (request.State == CouponState.HideOnline)
                    {
                        andfilters.Add(" Status = @Status ");
                        parameters.Add("Status", CouponInfoState.Online);
                        andfilters.Add(" IsHide = @IsHide ");
                        parameters.Add("IsHide", true);
                    }
                }
                if (request.ExpireTimeType != null)
                {
                    andfilters.Add("VaildDateType = @VaildDateType");
                    parameters.Add("VaildDateType", request.ExpireTimeType);
                    if (request.ExpireTimeType == CouponInfoVaildDateType.SpecialDate)
                    {

                        andfilters.Add(" (VaildStartDate >= @STime And VaildEndDate <= @ETime) ");
                        parameters.AddDynamicParams(new { request.STime, request.ETime });
                    }
                    if (request.ExpireTimeType == CouponInfoVaildDateType.SpecialDays)
                    {
                        andfilters.Add(" VaildTime = @VaildTime");
                        parameters.Add("VaildTime", TimeSpan.FromDays(request.ExpireDays.GetValueOrDefault()).TotalHours);
                    }
                }

                if (!string.IsNullOrEmpty(request.Title))
                {
                    andfilters.Add(" Name LIKE @title");
                    parameters.Add("title", $"%{request.Title}%");
                }

                if (!string.IsNullOrEmpty(request.EnableRangeKeyWord))
                {
                    andfilters.Add("  EXISTS (SELECT 1 FROM OPENJSON(KeyWord) WHERE [VALUE] LIKE @EnableRangeKeyWord) ");
                    parameters.Add("EnableRangeKeyWord", $"%{request.EnableRangeKeyWord}%");
                }
                filter = string.Join(" And ", andfilters);
            }

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
      ,[KeyWord]
      ,[Updator]
      ,[UpdateTime]
  FROM CouponInfo
  WHERE {0}
  ORDER BY CreateTime desc
  OFFSET @offset ROWS 
  FETCH NEXT @limit ROWS ONLY;
  SELECT count(1) FROM CouponInfo
  WHERE {0};
";

            var grid = await _orgUnitOfWork.QueryMultipleAsync(string.Format(sql, string.IsNullOrEmpty(filter) ? "1=1" : filter), parameters);
            var result = await grid.ReadAsync<CouponInfo>();
            int total = await grid.ReadFirstAsync<int>();
            grid.Dispose();
            var enableRangeSummaries = await _couponQueries.GetEnableRangeSummarys(result.Select(s => s.Id));
            var couponInfos = MapCouponInfos(result, enableRangeSummaries);
            couponInfos.Total = total;
            return couponInfos;
        }


        CouponInfos MapCouponInfos(IEnumerable<CouponInfo> result,IEnumerable<(Guid couponId, IEnumerable<EnableRangeSummary> enableRangeSummaries)> enableRangeSummariesTuples)
        {
            CouponInfos couponInfos = new CouponInfos();
            couponInfos.Items = new List<CouponInfoSummaryItem>();
            foreach (CouponInfo item in result)
            {
                var couponEnableRageSummaries = enableRangeSummariesTuples.FirstOrDefault(s => s.couponId == item.Id);
                couponInfos.Items.Add(MapCouponInfoSummaryItem(item, couponEnableRageSummaries.enableRangeSummaries));
            }
            return couponInfos;
        }

        CouponInfoSummaryItem MapCouponInfoSummaryItem(CouponInfo couponInfo,IEnumerable<EnableRangeSummary> enableRangeSummaries)
        {
            return new CouponInfoSummaryItem()
            {
                Id = couponInfo.Id,
                MaxTake = couponInfo.MaxTake,
                Title = couponInfo.Name,
                Number = CouponNumber.GetCouponNumberFromNumber((long)couponInfo.Number).ToString(),
                State = new CouponStateValue(couponInfo.Status, couponInfo.IsHide).CouponState,
                Total = couponInfo.Total,
                Stock = couponInfo.Stock,
                Type = couponInfo.CouponType,
                Value = CouponValue.GetCouponValue(couponInfo.CouponType, couponInfo.Fee, couponInfo.FeeOver, couponInfo.PriceOfTest, couponInfo.Discount),
                RuleDesc = couponInfo.Desc,
                ValidTime = new CouponVaildTimeFormatter((CouponInfoVaildDateType)couponInfo.VaildDateType, couponInfo.VaildStartDate, couponInfo.VaildEndDate, (long)couponInfo.VaildTime).BGListFormatt(),
                Discount = couponInfo.Discount,
                Fee = couponInfo.Fee,
                FeeOver = couponInfo.FeeOver,
                PriceOfTest = couponInfo.PriceOfTest,
                VaildDateType = couponInfo.VaildDateType,
                VaildDay = (int)TimeSpan.FromHours(couponInfo.VaildTime).TotalDays,
                VaildEndDate = couponInfo.VaildEndDate,
                VaildStartDate = couponInfo.VaildStartDate,
                EnableRangeSummaries = enableRangeSummaries
            };
        }

        CouponInfoItem MapCouponInfoItem(CouponInfo couponInfo) {
            return new CouponInfoItem()
            {
                Id = couponInfo.Id,
                MaxTake = couponInfo.MaxTake,
                Title = couponInfo.Name,
                Number = CouponNumber.GetCouponNumberFromNumber((long)couponInfo.Number).ToString(),
                State = new CouponStateValue(couponInfo.Status, couponInfo.IsHide).CouponState,
                Total = couponInfo.Total,
                Stock = couponInfo.Stock,
                Type = couponInfo.CouponType,
                Value = CouponValue.GetCouponValue(couponInfo.CouponType, couponInfo.Fee, couponInfo.FeeOver, couponInfo.PriceOfTest, couponInfo.Discount),
                RuleDesc = couponInfo.Desc,
                ValidTime = new CouponVaildTimeFormatter((CouponInfoVaildDateType)couponInfo.VaildDateType, couponInfo.VaildStartDate, couponInfo.VaildEndDate, (long)couponInfo.VaildTime).BGListFormatt(),
                EnableRange = couponInfo.GetEnableRanges(),
                Discount = couponInfo.Discount,
                Fee = couponInfo.Fee,
                FeeOver = couponInfo.FeeOver,
                PriceOfTest = couponInfo.PriceOfTest,
                VaildDateType = couponInfo.VaildDateType,
                VaildDay = (int)TimeSpan.FromHours(couponInfo.VaildTime).TotalDays,
                VaildEndDate = couponInfo.VaildEndDate,
                VaildStartDate = couponInfo.VaildStartDate
            };
        }





        IEnumerable<object> EnableRange_JSN2CouponEnableRanges(CouponEnableRangeType couponEnableRangeType, string enableRange_JSN)
        {
            IEnumerable<object> couponEnableRanges = new List<object>();
            if (string.IsNullOrEmpty(enableRange_JSN)) return couponEnableRanges;
            if (couponEnableRangeType == CouponEnableRangeType.SpecialGoods)
            {
                couponEnableRanges = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SKUEnableRange>>(enableRange_JSN);
            }
            if (couponEnableRangeType == CouponEnableRangeType.SpcialBrand)
            {
                couponEnableRanges = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CourseBrandEnableRange>>(enableRange_JSN);
            }
            if (couponEnableRangeType == CouponEnableRangeType.SpecialGoodsType)
            {
                couponEnableRanges = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GoodsTypeEnableRange>>(enableRange_JSN);
            }
            return couponEnableRanges;
        }
    }
}
