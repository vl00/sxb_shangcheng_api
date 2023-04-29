using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace iSchool.Organization.Appliaction.Queries
{
    public class GoodsQueries : IGoodsQueries
    {
        OrgUnitOfWork _orgUnitOfWork;
        private readonly IConfiguration _config;

        public GoodsQueries(IOrgUnitOfWork unitOfWork, IConfiguration config)
        {

            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._config = config;
        }

        public Task<DiscountAreaPglist> GetDiscountAreaContent(int index = 1, string couresName = "", int pageIndex = 1, int pageSize = 10)
        {
            var result = new DiscountAreaPglist();
            //读取配置文件中的优惠
            var couponList = _config.GetSection("AppSettings:CouponList")
                .GetChildren()
                .OrderBy(p => p.GetSection("FreeOver").Get<decimal>())
                .ThenBy(p => p.GetSection("Free").Get<decimal>())
                .Select((p, i) => new
                {
                    index = i + 1,
                    FreeOver = p.GetSection("FreeOver").Get<decimal>(),
                    Free = p.GetSection("Free").Get<decimal>(),
                    List = p.GetSection("List").Get<List<Guid>>()
                })

                .ToList();

            var couponids = couponList.FirstOrDefault(p => p.index == index)?.List;
            if (couponids == null || couponids.Count == 0)
            {
                throw new CustomResponseException("查询不到该优惠券的商品！");
            }

            //添加缓存（暂定）
            string sqlWhere = $@" where 1=1 and c.IsInvisibleOnline=0  and o.IsValid=1  and o.status=1 and c.status=1 and o.authentication=1 ";
            sqlWhere += " and c.LimitedTimeOffer=0 and c.NewUserExclusive=0 ";

            //搜索商品名字
            if (!string.IsNullOrEmpty(couresName))
            {
                sqlWhere += "  and c.title LIKE @coursename ";
            }

            //查询优惠券相关的商品
            string sql = $@" 
                        SELECT * from (select distinct	c.id,c.no,c.name,c.Title,isnull(c.banner_s,c.banner)as banner,c.price,c.origprice,c.stock,o.authentication,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.CreateTime,c.LastOnShelfTime,c.sellcount,c.CanNewUserReward,c.SetTop,c.NewUserExclusive ,c.ModifyDateTime from [dbo].[Course] c
                        	left join [dbo].[Organization]  o on o.id=c.orgid and c.IsValid=1  	
							LEFT join (SELECT id, value AS [types] FROM [Course]CROSS APPLY OPENJSON([GoodthingTypes]))	AA on  c.id=AA.id
                            LEFT JOIN dbo.CourseGoods AS CourseGood ON c.id=CourseGood.Courseid
							RIGHT JOIN ( SELECT sku.Id AS sukid
                FROM [Organization].[dbo].[CouponInfo] c
                    CROSS APPLY
                    OPENJSON(c.EnableRange_JSN, '$[0].SKUItems')
                    WITH
                    (
                        Id UNIQUEIDENTIFIER
                    ) sku
                WHERE c.Id in @Couponids
            ) couponSku
                ON couponSku.sukid = CourseGood.Id {sqlWhere}) c ORDER BY {(DateTime.Now.Day * 31 + index * 7) % 20 + 1 }
				 OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY";
            string sqlPage = $@"
                           	  SELECT  COUNT(1) FROM(  SELECT c.id FROM dbo.Course AS c
						 LEFT JOIN dbo.CourseGoods AS goods ON c.id=goods.Courseid 
						 LEFT JOIN dbo.Organization AS o ON o.id=c.orgid
						 RIGHT JOIN ( SELECT sku.Id AS sukid
                FROM [Organization].[dbo].[CouponInfo] c
                    CROSS APPLY
                    OPENJSON(c.EnableRange_JSN, '$[0].SKUItems')
                    WITH
                    (
                        Id UNIQUEIDENTIFIER
                    ) sku
                WHERE c.Id IN @Couponids
            ) couponSku
              ON couponSku.sukid = goods.Id {sqlWhere}  GROUP BY c.id) a";
            var data = new ResponseModels.CoursesByOrgIdQueryResponse();
            data.CoursesDatas = new List<ResponseModels.CoursesData>();
            var dBDatas = _orgUnitOfWork.Query<ResponseModels.CoursesDataDB>(sql, new { Couponids = couponids, coursename = $"%{couresName}%", PageIndex = pageIndex, pageSize = pageSize }).ToList();
            if (dBDatas != null)
            {
                for (int i = 0; i < dBDatas.Count; i++)
                {
                    var course = dBDatas[i];
                    var Tags = new List<string>();

                    //年龄标签
                    if (course.Minage != null && course.Maxage != null)
                    {
                        Tags.Add($"{course.Minage}-{course.Maxage}岁");
                    }
                    else if (course.Minage != null && course.Maxage == null)
                    {
                        Tags.Add($"大于{course.Minage}岁");
                    }
                    else if (course.Maxage != null && course.Minage == null)
                    {
                        Tags.Add($"小于{course.Maxage}岁");
                    }

                    ////科目标签
                    if (course.Subject != null)
                        Tags.Add(EnumUtil.GetDesc((Domain.Enum.SubjectEnum)course.Subject.Value));
                    //低价体验
                    if (course.Price <= 10)
                        Tags.Add("低价体验");
                    if (course.NewUserExclusive)
                        Tags.Add("新人专享");
                    if (course.CanNewUserReward)
                        Tags.Add("新人立返");
                    if (course.LimitedTimeOffer)
                        Tags.Add("限时补贴");
                    var addM = new ResponseModels.CoursesData()
                    {
                        Authentication = course.Authentication,
                        Banner = course.Banner == null ? null : JsonSerializationHelper.JSONToObject<List<string>>(course.Banner),
                        Id = course.Id,
                        Name = course.Name,
                        No = UrlShortIdUtil.Long2Base32(Convert.ToInt64(course.No)),
                        OrigPrice = course.OrigPrice,
                        Price = course.Price,
                        Stock = course.Stock,
                        Title = course.Title,
                        Tags = Tags,
                        LastOffShelfTime = course.LastOffShelfTime.UnixTicks(),
                        CanNewUserReward = course.CanNewUserReward

                    };
                    data.CoursesDatas.Add(addM);
                }
                data.PageInfo = new PageInfoResult();
                data.PageInfo.TotalCount = _orgUnitOfWork.QueryFirstOrDefault<int>(sqlPage, new { Couponids = couponids, coursename = $"%{couresName}%", PageIndex = pageIndex, pageSize = pageSize });
                data.PageInfo.PageIndex = pageIndex;
                data.PageInfo.PageSize = pageSize;
                data.PageInfo.TotalPage = (int)Math.Ceiling(data.PageInfo.TotalCount / (double)data.PageInfo.PageSize);
            }

            result.Index = index;
            result.CouponList = couponList.Select(p => new { index = p.index, FreeOver = p.FreeOver, Free = p.Free }).ToList();
            result.CourseList = data;
            return Task.FromResult(result);
        }

        public async Task<GoodsInfo> GetGoodsInfoAsync(Guid goodsId)
        {
            string sql = @"SELECT CourseGoods.Id,CourseGoods.Courseid CourseId,Course.No,Course.orgid, Course.[type] CourseType,Course.GoodthingTypes,Course.Subjects, Course.NewUserExclusive,Course.LimitedTimeOffer  FROM CourseGoods
JOIN Course ON Course.id = CourseGoods.Courseid
WHERE CourseGoods.IsValid = 1 AND Course.IsValid = 1 AND Course.id = @goodsId ";
            var res = await _orgUnitOfWork.QueryAsync(sql, new { goodsId }, _orgUnitOfWork.DbTransaction);
            return Map2GoodsInfo(res);
        }


        public async Task<SKUInfo> GetSKUInfoAsync(Guid skuId)
        {
            string sql = @"  SELECT CourseGoods.Id Id, CourseGoods.[Price] UnitPrice,Course.orgid BrandId, Course.[type] CourseType,Course.GoodthingTypes,Course.Subjects FROM CourseGoods
  JOIN Course ON CourseGoods.Courseid = Course.id
  WHERE  CourseGoods.Id = @skuId ";
            var res = await _orgUnitOfWork.QueryFirstOrDefault(sql, new { skuId }, _orgUnitOfWork.DbTransaction);
            return Map2SKUInfo(res);
        }

        public async Task<IEnumerable<SKUInfo>> GetSKUInfosAsync(IEnumerable<Guid> skuIds)
        {
            string sql = @"  SELECT CourseGoods.Id Id,CourseGoods.[Price] UnitPrice,Course.orgid BrandId, Course.[type] CourseType, Course.GoodthingTypes ,Course.Subjects, Course.NewUserExclusive,Course.LimitedTimeOffer FROM CourseGoods
  JOIN Course ON CourseGoods.Courseid = Course.id
  WHERE  CourseGoods.Id in @skuIds ";
            var res = await _orgUnitOfWork.QueryAsync(sql, new { skuIds }, _orgUnitOfWork.DbTransaction);
            return res.Select<dynamic, SKUInfo>(s => Map2SKUInfo(s));
        }

        public Task<IEnumerable<Goods>> SearchGoods(IEnumerable<Guid> skuIds, IEnumerable<Guid> brandIds, IEnumerable<int> goodTypes, string searchText = null, int offset = 0, int limit = 20)
        {
            string sql = @"
SELECT Course.id
,Course.[No]
,Course.title
,Course.banner_s BannerThumbnailsJsn
,Course.banner BannersJsn
,Course.price
,Course.origprice
,Course.[subject]
,Course.minage
,Course.maxage
,Course.NewUserExclusive
,Course.CanNewUserReward
,Course.LimitedTimeOffer
,Course.GoodthingTypes
FROM Course
{0}
ORDER BY CreateTime DESC
OFFSET @offset ROWS 
FETCH NEXT @limit ROWS ONLY";

            List<string> andFilter = new List<string>() { "IsValid = 1", " [status] =1", "IsInvisibleOnline =0", " NewUserExclusive = 0", "LimitedTimeOffer = 0" };
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("offset", offset);
            parameters.Add("limit", limit);
            if (!string.IsNullOrEmpty(searchText))
            {
                andFilter.Add("Course.title LIKE @searchText");
                parameters.Add("searchText", $"%{searchText}%");
            }
            List<string> orFilter = new List<string>();
            if (brandIds != null && brandIds.Any())
            {
                orFilter.Add("orgid IN @brandIds");
                parameters.Add("brandIds", brandIds);
            }
            if (skuIds != null && skuIds.Any())
            {
                orFilter.Add("EXISTS (SELECT 1 FROM CourseGoods WHERE CourseGoods.Courseid = Course.id AND CourseGoods.Id IN @skuIds)");
                parameters.Add("skuIds", skuIds);
            }
            if (goodTypes != null && goodTypes.Any())
            {
                orFilter.Add("EXISTS(SELECT 1 FROM OPENJSON(Course.GoodthingTypes) WHERE [VALUE] IN @goodTypes)");
                orFilter.Add("EXISTS(SELECT 1 FROM OPENJSON(Course.Subjects) WHERE [VALUE] IN @goodTypes)");
                parameters.Add("goodTypes", goodTypes);
            }
            if (orFilter.Any()) andFilter.Add(string.Join(" OR ", orFilter));
            if (andFilter.Any()) sql = sql.FormatWith($"  WHERE {string.Join(" AND ", andFilter)}");

            var res = _orgUnitOfWork.QueryAsync<Goods>(sql, parameters, _orgUnitOfWork.DbTransaction);
            return res;
        }

        GoodsInfo Map2GoodsInfo(IEnumerable<dynamic> res)
        {
            if (!res.Any()) return null;

            var first = res.First();
            GoodsInfo goodsInfo = new GoodsInfo()
            {
                Id = first.CourseId,
                No = first.No,
                BrandId = first.orgid,
                LimitedTimeOffer = first.LimitedTimeOffer,
                NewUserExclusive = first.NewUserExclusive,
                SKUIds = res.Select(r => (Guid)r.Id).ToList(),
                GoodsTypeIds = new List<int>()
            };

            if (first.CourseType == 1)
            {
                //网课
                if (!string.IsNullOrEmpty(first.Subjects))
                {
                    goodsInfo.GoodsTypeIds.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(first.Subjects));

                }
            }
            else if (first.CourseType == 2)
            {
                //好物

                if (!string.IsNullOrEmpty(first.GoodthingTypes))
                {
                    goodsInfo.GoodsTypeIds.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(first.GoodthingTypes));
                }

            }

            return goodsInfo;
        }

        SKUInfo Map2SKUInfo(dynamic res)
        {
            if (res == null) return null;
            SKUInfo skuInfo = new SKUInfo()
            {
                Id = res.Id,
                BrandId = res.BrandId,
                UnitPrice = res.UnitPrice,
                GoodsTypeIds = new List<int>(),
                LimitedTimeOffer = res.LimitedTimeOffer,
                NewUserExclusive = res.NewUserExclusive

            };
            if (res.CourseType == 1)
            {
                //网课
                if (!string.IsNullOrEmpty(res.Subjects))
                {
                    skuInfo.GoodsTypeIds.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(res.Subjects));

                }
            }
            else if (res.CourseType == 2)
            {
                //好物
                if (!string.IsNullOrEmpty(res.GoodthingTypes))
                {
                    skuInfo.GoodsTypeIds.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(res.GoodthingTypes));
                }
            }




            return skuInfo;
        }


    }

    public class SKUInfo
    {

        public Guid Id { get; set; }

        public Guid BrandId { get; set; }

        public decimal UnitPrice { get; set; }

        public List<int> GoodsTypeIds { get; set; }


        public bool NewUserExclusive { get; set; }
        public bool LimitedTimeOffer { get; set; }

    }

    public class GoodsInfo
    {

        public Guid Id { get; set; }
        public long No { get; set; }

        public bool NewUserExclusive { get; set; }
        public bool LimitedTimeOffer { get; set; }

        public List<Guid> SKUIds { get; set; }

        public Guid BrandId { get; set; }

        public List<int> GoodsTypeIds { get; set; }

    }

}
