using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.SchoolsApis
{
    public class HotSellCoursesOrgsForSchoolsQueryHandler : IRequestHandler<HotSellCoursesOrgsForSchoolsQuery, HotSellCoursesOrgsForSchoolsQryResult>
    {
        IConfiguration _config;
        CSRedisClient _redis;
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;

        const int SizeLen = 20;

        public HotSellCoursesOrgsForSchoolsQueryHandler(IConfiguration config, CSRedisClient redis, IOrgUnitOfWork orgUnitOfWork,
            IMediator mediator)
        {
            this._config = config;
            this._redis = redis;
            this._mediator = mediator;
            this._orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
        }

        public async Task<HotSellCoursesOrgsForSchoolsQryResult> Handle(HotSellCoursesOrgsForSchoolsQuery query, CancellationToken cancellation)
        {
            HotSellCoursesOrgsForSchoolsQryResult result = null;            

            if (query.MinAge < 0 || query.MaxAge < 0)
            {
                throw new CustomResponseException("参数错误");
            }
            if (query.MinAge > query.MaxAge)
            {
                throw new CustomResponseException("参数错误");
            }

            var cacheKey = CacheKeys.Toschool_HotsellCourses.FormatWith(query.MinAge, query.MaxAge);
            result = await _redis.GetAsync<HotSellCoursesOrgsForSchoolsQryResult>(cacheKey);
            if (result?.Time == null)
            {
                result = new HotSellCoursesOrgsForSchoolsQryResult { Time = DateTime.Now };
                {
                    var r_HotSellCourses = new List<PcCourseItemDto2>(SizeLen); // 热卖课程
                    var r_RecommendOrgs = new List<PcOrgItemDto>(SizeLen);      // 推荐机构

                    // 热卖课程                    
                    {
                        var sql = $@"
select top {SizeLen} o.authentication,o.name as orgname,c.id as __id,c.*
from [course] c left join [Organization] o on o.id=c.orgid
where c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and c.type={CourseTypeEnum.Course.ToInt()} and c.IsInvisibleOnline=0
and o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} and o.[authentication]=1
{"and not(c.maxage<@MinAge or c.minage>@MaxAge)".If(query.MinAge > 0 && query.MaxAge > 0)}
order by c.sellcount desc,c.createtime desc
";
                        var citems = await _orgUnitOfWork.QueryAsync<(bool, string), Course, PcCourseItemDto2>(sql,
                            splitOn: "__id",
                            param: new { query.MinAge, query.MaxAge },
                            map: (lg, course) =>
                            {
                                var item = new PcCourseItemDto2();
                                item.Id = course.Id;
                                item.Id_s = UrlShortIdUtil.Long2Base32(course.No);
                                item.Title = course.Title;
                                item.Subtitle = course.Subtitle;
                                item.Banner = course.Banner.IsNullOrEmpty() ? null : course.Banner?.ToObject<string[]>()?.ElementAtOrDefault(0);
                                item.Price = course.Price;
                                item.OrigPrice = course.Origprice;
                                item.Authentication = lg.Item1;
                                item.OrgName = lg.Item2;
                                item.Sellcount = course.Sellcount ?? 0;
                                item.IsExplosions = course.IsExplosions ?? false;
                                item.Tags = OrderHelper.GetTagsFromCourse(course);
                                return item;
                            }
                        );                        
                        r_HotSellCourses.AddRange(citems);
                    }

                    // 推荐机构
                    do
                    {
                        var sql = $@"
select top {SizeLen} c.orgid
from [course] c left join [Organization] o on o.id=c.orgid
where c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and c.type={CourseTypeEnum.Course.ToInt()} and c.IsInvisibleOnline=0
and o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} and o.[authentication]=1
{"and not(c.maxage<@MinAge or c.minage>@MaxAge)".If(query.MinAge > 0 && query.MaxAge > 0)}
group by c.orgid order by sum(c.sellcount) desc
";
                        var ls_orgIds = await _orgUnitOfWork.QueryAsync<Guid>(sql, new { query.MinAge, query.MaxAge });

                        if (ls_orgIds.Count() < 1) break;

                        sql = $@"
select o.no,o.id,o.name,o.logo,o.authentication,o.[desc],o.subdesc 
from [Organization] o 
where o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} and o.[authentication]=1
{(ls_orgIds.Count() > 0 ? $"and o.id in @ls_orgIds" : "")}
";
                        var oitems = await _orgUnitOfWork.QueryAsync<long, PcOrgItemDto, PcOrgItemDto>(sql,
                            splitOn: "id",
                            param: new { ls_orgIds },
                            map: (no, item) =>
                            {
                                item.Id_s = UrlShortIdUtil.Long2Base32(no);
                                return item;
                            }
                        );
                        r_RecommendOrgs.AddRange(oitems);
                    }
                    while (false);

                    result.HotSellCourses = r_HotSellCourses;
                    result.RecommendOrgs = r_RecommendOrgs;
                }
                await _redis.SetAsync(cacheKey, result, 60 * 8);
            }

            var random = new Random(DateTime.Now.Millisecond);
            result.HotSellCourses = GetRandItems(result.HotSellCourses, random, 3).OrderByDescending(_ => _.Sellcount).ToArray();
            result.RecommendOrgs = GetRandItems(result.RecommendOrgs, random, 3);
            {
                var dict = await _mediator.Send(new PcGetOrgsCountsQuery { OrgIds = result.RecommendOrgs.Select(_ => _.Id) });
                foreach (var item in result.RecommendOrgs)
                {
                    if (!dict.TryGetValue(item.Id, out var m)) continue;
                    item.CourceCount = m.CourceCount;
                    item.EvaluationCount = m.EvaluationCount;
                    item.GoodsCount = m.GoodsCount;
                }
            }
            result.RecommendOrgs = result.RecommendOrgs.OrderByDescending(_ => _.CourceCount).ToArray();

            return result;
        }


        static T[] GetRandItems<T>(IEnumerable<T> items, Random random, int len)
        {
            if (items.Count() <= len) return items.AsArray();
            var arr = new int[len];
            for (var i = 0; i < len; i++)
            {
                var j = random.Next(0, items.Count());
                if (arr.Contains(j)) 
                {
                    i--;
                    continue;
                }
                arr[i] = j;
            }
            return arr.Select(i => items.ElementAtOrDefault(i)).ToArray();
        }
    }
}
