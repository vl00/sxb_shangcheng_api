using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class MpMallThemeDetailQueryHandler : IRequestHandler<MpMallThemeDetailQuery, MpMallThemeDetailQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public MpMallThemeDetailQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<MpMallThemeDetailQryResult> Handle(MpMallThemeDetailQuery query, CancellationToken cancellation)
        {
            var result = new MpMallThemeDetailQryResult();
            await default(ValueTask);

            switch (1)
            {
                // tid & spid == default // 本期主题的默认专题
                case 1 when query.Tid.IsNullOrEmpty() && query.Spid.IsNullOrEmpty():
                    {
                        var sql = @"select top 1 t.* from MallThemes t where t.IsValid=1 and t.StartTime<=getdate() and t.endtime>=getdate() order by t.StartTime ";
                        var theme = await _orgUnitOfWork.QueryFirstOrDefaultAsync<MallThemes>(sql);
                        if (theme == null) throw new CustomResponseException("找不到本期主题");
                        Map(ref result, theme);

                        sql = $@"select top 1 * from MallSpecials where IsValid=1 and ThemeId=@Tid order by sort,Id ";
                        var special = await _orgUnitOfWork.QueryFirstOrDefaultAsync<MallSpecials>(sql, new { result.Tid });
                        if (special == null) throw new CustomResponseException($"找不到主题'{result.Tname}'的默认专题");
                        Map(ref result, special);
                    }
                    break;

                // tid != null && spid == null // 固定主题的默认专题
                case 1 when !query.Tid.IsNullOrEmpty() && query.Spid.IsNullOrEmpty():
                    { 
                        var tid = Guid.TryParse(query.Tid, out var _tid) ? _tid : default;
                        var tno = tid == default ? UrlShortIdUtil.Base322Long(query.Tid) : default;

                        var sql = $@"select t.* from MallThemes t where t.IsValid=1 {"and id=@tid".If(tid != default)} {"and no=@tno".If(tno != default)} ";
                        var theme = await _orgUnitOfWork.QueryFirstOrDefaultAsync<MallThemes>(sql, new { tid, tno });
                        if (theme == null) throw new CustomResponseException("找不到该主题");
                        Map(ref result, theme);

                        sql = $@"select top 1 * from MallSpecials where IsValid=1 and ThemeId=@Tid order by sort,Id ";
                        var special = await _orgUnitOfWork.QueryFirstOrDefaultAsync<MallSpecials>(sql, new { result.Tid });
                        if (special == null) throw new CustomResponseException($"找不到主题'{result.Tname}'的默认专题");
                        Map(ref result, special);
                    }
                    break;

                // tid != null && spid != null || spid != null // 固定主题的固定专题
                case 1 when !query.Spid.IsNullOrEmpty():
                    {
                        var spid = Guid.TryParse(query.Spid, out var _spid) ? _spid : default;
                        var spno = spid == default ? UrlShortIdUtil.Base322Long(query.Spid) : default;

                        var sql = $@"select * from MallSpecials where IsValid=1 {"and id=@spid".If(spid != default)} {"and no=@spno".If(spno != default)} ";
                        var special = await _orgUnitOfWork.QueryFirstOrDefaultAsync<MallSpecials>(sql, new { spid, spno });
                        if (special == null) throw new CustomResponseException($"找不到该专题");
                        Map(ref result, special);

                        var tid = default(Guid);
                        var tno = default(long);
                        if (!query.Tid.IsNullOrEmpty())
                        {
                            tid = Guid.TryParse(query.Tid, out var _tid) ? _tid : default;
                            tno = tid == default ? UrlShortIdUtil.Base322Long(query.Tid) : default;
                        }                        
                        {
                            sql = $@"select t.* from MallThemes t where t.IsValid=1 and t.id=@ThemeId ";
                            var theme = await _orgUnitOfWork.QueryFirstOrDefaultAsync<MallThemes>(sql, new { special.ThemeId });
                            if (theme == null) throw new CustomResponseException("找不到该主题");
                            if (!query.Tid.IsNullOrEmpty() && theme.Id != tid && theme.No != tno) throw new CustomResponseException($"主题与专题不匹配");
                            Map(ref result, theme);
                        }
                    }
                    break;

                default:
                    throw new CustomResponseException("no support");
            }

            // IsThemesLessThan3
            {
                var themesCounts = await _redis.GetAsync<int?>(CacheKeys.MallThemes_counts);
                if (themesCounts == null)
                {
                    var sql = "select count(1) from MallThemes where isvalid=1";
                    themesCounts = await _orgUnitOfWork.QueryFirstOrDefaultAsync<int>(sql);

                    await _redis.SetAsync(CacheKeys.MallThemes_counts, themesCounts, 60 * 60 * 2);
                }
                result.IsThemesLessThan3 = (themesCounts ?? 0) < 3;
            }

            // 主题下的专题s
            {
                var specials = await _redis.GetAsync<MpMallThemeDetailQryResult_Special[]>(CacheKeys.MallThemes_Theme_specials.FormatWith(result.Tid));
                if (specials == null)
                {
                    var sql = @" 
select id as spid,no as spid_s,name as spname,banner as spbanner,sbanner as spbanner_s,ShareText
from MallSpecials where IsValid=1 and ThemeId=@Tid order by sort 
";
                    specials = (await _orgUnitOfWork.QueryAsync<MpMallThemeDetailQryResult_Special>(sql, new { result.Tid })).AsArray();
                    foreach (var dto in specials)
                    {
                        dto.Spid_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(dto.Spid_s));
                    }

                    await _redis.SetAsync(CacheKeys.MallThemes_Theme_specials.FormatWith(result.Tid), specials, 60 * 60 * 1);
                }
                result.Specials = specials;
            }

            // 专题的概念图片-锚点s
            {
                var concepts = await _redis.GetAsync<MpMallThemeDetailQryResult_Concept[]>(CacheKeys.MallThemes_Special_concepts.FormatWith(result.Spid));
                if (concepts == null)
                {
                    var sql = $@"
select c.no,sc.* from MallSpecialCourses sc join course c on sc.courseid=c.id
where sc.IsValid=1 and c.isvalid=1 and c.status=1 
and sc.type=1 and sc.SpecialId=@Spid
order by sc.sort
";
                    concepts = (await _orgUnitOfWork.QueryAsync<long, MallSpecialCourses, MpMallThemeDetailQryResult_Concept>(sql, splitOn: "id",
                        param: new { result.Spid },
                        map: (no, mallSpecialCourses) =>
                        {
                            var dto = new MpMallThemeDetailQryResult_Concept();
                            dto.CourseId = mallSpecialCourses.CourseId;
                            dto.CourseId_s = UrlShortIdUtil.Long2Base32(no);
                            dto.Shape = mallSpecialCourses.Shape;
                            dto.Coords = mallSpecialCourses.Coords;
                            return dto;
                        }
                    )).AsArray();

                    await _redis.SetAsync(CacheKeys.MallThemes_Special_concepts.FormatWith(result.Spid), concepts, 60 * 60 * 1);
                }
                result.Concepts = concepts;

                foreach (var concept in concepts)
                {
                    concept.Href = $"{_config["AppSettings:CreateMpQrcode:course-detail:Page"]}?id={concept.CourseId_s}";
                }
            }

            // 专题下商品s
            {
                var courses = await _redis.GetAsync<MpCourseDataDto[]>(CacheKeys.MallThemes_Special_courses.FormatWith(result.Spid));
                if (courses == null)
                {
                    var sql = $@"
select o.authentication,c.id,c.no,isnull(c.banner_s,c.banner)as banner,c.Title,c.price,c.origprice,c.stock,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,c.sellcount,c.IsExplosions,c.CanNewUserReward,c.NewUserExclusive,c.LimitedTimeOffer
from MallSpecialCourses sc join course c on sc.courseid=c.id join [dbo].[Organization] o on o.id=c.orgid and o.IsValid=1 and o.status=1 and o.Authentication=1
where sc.IsValid=1 and c.isvalid=1 and c.status=1 and isnull(c.IsInvisibleOnline,0)=0
and sc.type=2 and sc.SpecialId=@Spid
order by sc.sort
";
                    courses = (await _orgUnitOfWork.QueryAsync<bool, Course, MpCourseDataDto>(sql, splitOn: "id", param: new { result.Spid },
                        map: (authentication, course) =>
                        {
                            var dto = new MpCourseDataDto();
                            dto.Id = course.Id;
                            dto.Id_s = UrlShortIdUtil.Long2Base32(course.No);
                            dto.Banner = course.Banner?.ToObject<List<string>>();
                            dto.Authentication = authentication;
                            dto.Title = course.Title;
                            dto.Price = course.Price ?? 0;
                            dto.OrigPrice = course.Origprice;
                            dto.Stock = course.Stock ?? 0;
                            dto.LastOffShelfTime = course.LastOffShelfTime;
                            dto.NewUserExclusive = course.NewUserExclusive;
                            dto.CanNewUserReward = course.CanNewUserReward;
                            dto.LimitedTimeOffer = course.LimitedTimeOffer;
                            dto.Tags = OrderHelper.GetTagsFromCourse(course);
                            return dto;
                        })
                    ).AsArray();

                    await _redis.SetAsync(CacheKeys.MallThemes_Special_courses.FormatWith(result.Spid), courses, 60 * 60 * 1);
                }
                result.Courses = courses;
            }

            return result;
        }


        static void Map(ref MpMallThemeDetailQryResult result, MallThemes theme)
        {
            result.Tid = theme.Id;
            result.Tid_s = UrlShortIdUtil.Long2Base32(theme.No);
            result.Tname = theme.Name;
            result.Tlogo = theme.Logo;
            result.Tbanner = theme.Banner?.ToObject<string[]>()?.FirstOrDefault();
        }

        static void Map(ref MpMallThemeDetailQryResult result, MallSpecials special)
        {            
            result.Spid = special.Id;
            result.Spid_s = UrlShortIdUtil.Long2Base32(special.No);
            result.Spname = special.Name;
            result.Spbanner = special.Banner;
            result.Background = special.MBackground;
            result.SpShareText = special.ShareText;
        }
    }
}
