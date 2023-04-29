using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
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
    public class GG21QueryHandler : IRequestHandler<GG21Query, GG21QryResult>
    {
        IConfiguration _config;
        CSRedisClient _redis;
        IMediator _mediator;
        OrgUnitOfWork _orgUnitOfWork;

        const int SizeLen = 21;

        public GG21QueryHandler(IConfiguration config, CSRedisClient redis, IOrgUnitOfWork orgUnitOfWork,
            IMediator mediator)
        {
            this._config = config;
            this._redis = redis;
            this._mediator = mediator;
            this._orgUnitOfWork = orgUnitOfWork as OrgUnitOfWork;
        }

        public async Task<GG21QryResult> Handle(GG21Query query, CancellationToken cancellation)
        {
            GG21QryResult result = default!;

            var subjArr = query.Subjs.IsNullOrEmpty() ? (int[])null
                : query.Subjs.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).OrderBy(_ => _).ToArray();

            var ages = query.Ages.Where(_ => !((_.MinAge ?? 0) == 0 && (_.MaxAge ?? 0) == 0) && _.MinAge >= 0 && _.MaxAge >= 0)
                .OrderBy(_ => _.MinAge).ToArray();

            var cacheKey = CacheKeys.Toschool_gg21.FormatWith(string.Join("or", ages.Select(_ => $"{_.MinAge},{_.MaxAge}")), query.Price, (subjArr == null ? null : string.Join(',', subjArr)));
            result = await _redis.GetAsync<GG21QryResult>(cacheKey);
            if (result?.Time == null)
            {
                result = new GG21QryResult { Time = DateTime.Now };
                {
                    var sql = $@"
select top {SizeLen} * from(
select c.no,o.id as orgid,c.id,o.name as orgname,c.title,c.subtitle,c.banner,c.price,c.origprice,o.authentication,c.sellcount,isnull(c.IsExplosions,0)as IsExplosions,c.viewcount, --c.Subject,c.Subjects,
(({{0}})+
 ({{1}})+
 ({{2}}))as _qz
from [course] c left join [Organization] o on o.id=c.orgid
where c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and c.IsInvisibleOnline=0
and o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} and o.[authentication]=1
)T order by _qz desc,IsExplosions desc,sellcount desc,viewcount desc
";
                    //var str_age = query.MinAge > 0 && query.MaxAge > 0 ? "case when not(c.maxage<@MinAge or c.minage>@MaxAge) then 1 else 0 end" : "1";
                    var str_age = query.Ages?.Length > 0
                        ? $"case when ({(string.Join(" or ", query.Ages.Select(_ => $"not(c.maxage<{_.MinAge} or c.minage>{_.MaxAge})")))}) then 1 else 0 end"
                        : "1";
                    var str_subjs = subjArr?.Length > 0 
                        ? $"case when exists(select 1 from openjson(isnull(c.Subjects,N'[{SubjectEnum.Other.ToInt()}]')) j1 join openjson(N'{subjArr.ToJsonString()}') j2 on j1.[value]=j2.[value]) then 1 else 0 end" 
                        : "1";
                    var str_price = query.Price switch 
                    {
                        1 => "case when c.price>=100 then 1 else 0 end",
                        2 => "case when c.price<100 then 1 else 0 end",
                        _ => "1",
                    };
                    sql = string.Format(sql, str_age, str_subjs, str_price);

                    var citems = await _orgUnitOfWork.QueryAsync<(long, Guid), PcCourseItemDto2, PcCourseItemDto2>(sql,
                        splitOn: "id",
                        param: new { },
                        map: (lg, item) =>
                        {
                            item.Banner = item.Banner.IsNullOrEmpty() ? null : item.Banner?.ToObject<string[]>()?.ElementAtOrDefault(0);
                            item.Id_s = UrlShortIdUtil.Long2Base32(lg.Item1);
                            return item;
                        }
                    );
                    result.Courses = citems.AsArray();
                }
                await _redis.SetAsync(cacheKey, result, 60 * 60 * 24);
            }

            // urls
            {
                foreach (var item in result.Courses)
                {
                    item.MUrl = $"{_config["BaseUrls:org-m"]}/course/detail/{item.Id_s}";
                    item.PcUrl = $"{_config["BaseUrls:org-pc"]}/course/detail/{item.Id_s}";

                    // mpqrcode
                    if (query.Mp)
                    {
                        try
                        {
                            var cmd1 = _config.GetSection("AppSettings:CreateMpQrcode:course-detail").Get<CreateMpQrcodeCmd>();
                            cmd1.Scene = cmd1.Scene.FormatWith(item.Id_s);
                            item.MpQrcode = (await _mediator.Send(cmd1)).MpQrcode;
                        }
                        catch { /* ignore */ }
                    }
                }
            }

            return result;
        }


    }
}
