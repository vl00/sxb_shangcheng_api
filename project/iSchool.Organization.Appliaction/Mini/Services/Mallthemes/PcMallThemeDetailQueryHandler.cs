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
    public class PcMallThemeDetailQueryHandler : IRequestHandler<PcMallThemeDetailQuery, PcMallThemeDetailQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public PcMallThemeDetailQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<PcMallThemeDetailQryResult> Handle(PcMallThemeDetailQuery query, CancellationToken cancellation)
        {
            var result = new PcMallThemeDetailQryResult();
            var now = DateTime.Now;
            await default(ValueTask);

            MallThemes theme = null;
            switch (1)
            {
                // 本期主题
                case 1 when query.Tid.IsNullOrEmpty():
                    {
                        theme = await GetCurrMallThemes();
                        if (theme == null) throw new CustomResponseException("找不到本期主题");
                        Map(ref result, theme);
                    }
                    break;

                // 固定主题
                case 1 when !query.Tid.IsNullOrEmpty():
                    {
                        var tid = Guid.TryParse(query.Tid, out var _tid) ? _tid : default;
                        var tno = tid == default ? UrlShortIdUtil.Base322Long(query.Tid) : default;

                        var sql = $@"select t.* from MallThemes t where t.IsValid=1 {"and id=@tid".If(tid != default)} {"and no=@tno".If(tno != default)} ";
                        theme = await _orgUnitOfWork.QueryFirstOrDefaultAsync<MallThemes>(sql, new { tid, tno });
                        if (theme == null) throw new CustomResponseException("找不到该主题");
                        Map(ref result, theme);
                    }
                    break;
            }

            // 主题下的专题s
            {
                var specials = await _redis.GetAsync<PcMallThemeDetailQryResult_Special[]>(CacheKeys.MallThemes_Theme_pc_specials.FormatWith(result.Tid));
                if (specials == null)
                {
                    var sql = @" 
select id as spid,no as spid_s,name as spname,banner as spbanner,sbanner as spbanner_s,ConceptPicture,ShareText
from MallSpecials where IsValid=1 and ThemeId=@Tid order by sort 
";
                    specials = (await _orgUnitOfWork.QueryAsync<PcMallThemeDetailQryResult_Special>(sql, new { result.Tid })).AsArray();
                    foreach (var dto in specials)
                    {
                        dto.Spid_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(dto.Spid_s));
                        dto.Courses = new List<MpCourseDataDto>();
                    }

                    await _redis.SetAsync(CacheKeys.MallThemes_Theme_pc_specials.FormatWith(result.Tid), specials, 60 * 60 * 1);
                }
                result.Specials = specials;
            }
            // 各个专题下的商品s
            if (result.Specials?.Length > 0)
            {
                var courses = await _redis.GetAsync<Dto_SpecialCourse[]>(CacheKeys.MallThemes_Theme_pc_specials_courses.FormatWith(result.Tid));
                if (courses == null)
                {
                    var sql = $@"
select sc.sort,sc.SpecialId,o.authentication,c.id,c.no,isnull(c.banner_s,c.banner)as banner,c.Title,c.price,c.origprice,c.stock,c.minage,c.maxage,c.subject,c.LastOffShelfTime,c.LastOnShelfTime,c.sellcount,c.IsExplosions,c.CanNewUserReward,c.NewUserExclusive,c.LimitedTimeOffer
from MallSpecialCourses sc join course c on sc.courseid=c.id join [dbo].[Organization] o on o.id=c.orgid and o.IsValid=1 and o.status=1 and o.Authentication=1
where sc.IsValid=1 and c.isvalid=1 and c.status=1 and isnull(c.IsInvisibleOnline,0)=0
and sc.type=2 and sc.SpecialId in @spids
";
                    courses = (await _orgUnitOfWork.QueryAsync<(int, Guid), bool, Course, Dto_SpecialCourse>(sql, splitOn: "authentication,id",
                        param: new { spids = result.Specials.Select(_ => _.Spid).ToArray() },
                        map: (itm, authentication, course) =>
                        {
                            var dto0 = new Dto_SpecialCourse();
                            dto0.Sort = itm.Item1;
                            dto0.Spid = itm.Item2;
                            var dto = dto0.MpCourseDataDto = new MpCourseDataDto();
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
                            return dto0;
                        })
                    ).AsArray();

                    await _redis.SetAsync(CacheKeys.MallThemes_Theme_pc_specials_courses.FormatWith(result.Tid), courses, 60 * 60 * 1);
                }

                foreach (var special in result.Specials)
                {
                    special.Courses = courses.Where(_ => _.Spid == special.Spid).OrderBy(_ => _.Sort).Select(_ => _.MpCourseDataDto).ToList();
                }
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
            // 下期主题
            {
//                var nextTheme = await _redis.GetAsync<MallThemes>(CacheKeys.MallThemes_Theme_NextTheme.FormatWith(result.Tid));
//                if (nextTheme == null)
//                {
//                    var sql = @"
//select top 1 t.* from MallThemes t where t.isvalid=1 and t.StartTime>(select StartTime from MallThemes where id=@Tid)
//order by t.StartTime
//";
//                    nextTheme = await _orgUnitOfWork.QueryFirstOrDefaultAsync<MallThemes>(sql, new { result.Tid });
//                    nextTheme ??= new MallThemes();

//                    await _redis.SetAsync(CacheKeys.MallThemes_Theme_NextTheme.FormatWith(result.Tid), nextTheme, 60 * 60 * 3);
//                }
//                if (nextTheme.Id != default)
//                {
//                    result.NextTid_s = UrlShortIdUtil.Long2Base32(nextTheme.No);
//                    result.NextTlogo = nextTheme.Logo;
//                }
            }
            // 本期主题 与 本期的上期主题
            {
                // 本期主题
                MallThemes currTheme = null;
                if (theme.StartTime != null && theme.StartTime <= now && (theme.EndTime == null || theme.EndTime >= now))
                {
                    currTheme = theme;
                }
                else
                {
                    currTheme = await GetCurrMallThemes();
                }
                result.CurrTid_s = currTheme == null ? null : UrlShortIdUtil.Long2Base32(currTheme.No);
                result.CurrTlogo = currTheme?.Logo;

                // 本期的上期主题
                if (currTheme?.StartTime != null)
                {
                    var prevCurrTheme = await GetPrevMallThemes(currTheme.StartTime.Value);
                    result.PrevCurrTid_s = prevCurrTheme == null ? null : UrlShortIdUtil.Long2Base32(prevCurrTheme.No);
                    result.PrevCurrTlogo = prevCurrTheme?.Logo;
                }
            }

            return result;
        }

        async Task<MallThemes> GetCurrMallThemes()
        {
            var sql = @"select top 1 t.* from MallThemes t where t.IsValid=1 and t.StartTime<=getdate() and t.endtime>=getdate() order by t.StartTime ";
            var theme = await _orgUnitOfWork.QueryFirstOrDefaultAsync<MallThemes>(sql);
            return theme;
        }

        async Task<MallThemes> GetPrevMallThemes(DateTime time)
        {
            var sql = @"select top 1 t.* from MallThemes t where t.IsValid=1 and t.StartTime<=@time and t.endtime<@time order by t.StartTime desc ";
            var theme = await _orgUnitOfWork.QueryFirstOrDefaultAsync<MallThemes>(sql, new { time });
            return theme;
        }

        static void Map(ref PcMallThemeDetailQryResult result, MallThemes theme)
        {
            result.Tid = theme.Id;
            result.Tid_s = UrlShortIdUtil.Long2Base32(theme.No);
            result.Tname = theme.Name;
            result.Tlogo = theme.Logo;
            result.Tbanner = theme.Banner?.ToObject<string[]>()?.FirstOrDefault();
            result.Background = theme.PcBackground;
        }

        class Dto_SpecialCourse
        {
            public Guid Spid { get; set; }
            public int Sort { get; set; }
            public MpCourseDataDto MpCourseDataDto { get; set; }
        }
    }
}
