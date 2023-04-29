using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class GetHotSellCoursesQueryHandler : IRequestHandler<GetHotSellCoursesQuery, GetHotSellCoursesQueryResult>
    {
        OrgUnitOfWork _unitOfWork;
        IMediator _mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper _mapper;
        IConfiguration _config;

        public GetHotSellCoursesQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IUserInfo me, IConfiguration config,
            IMapper mapper)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<GetHotSellCoursesQueryResult> Handle(GetHotSellCoursesQuery query, CancellationToken cancellation)
        {
            var result = new GetHotSellCoursesQueryResult();            

            //result.Subjs = GetSubjs().Select(_ => new SelectItemsKeyValues { Key = _, Value = EnumUtil.GetDesc((SubjectEnum)_) }).AsList();

            result.PageInfo = await GetPagedList(query);

            return result;
        }

        async Task<PagedList<PcCourseItemDto2>> GetPagedList(GetHotSellCoursesQuery query)
        {
            PcCourseItemDto2[] items = null;
            int cc = 0;
            await default(ValueTask);

            // redis key
            var rdk = CacheKeys.Toschoolsv2_HotsellCourses.FormatWith(query.PageSize, query.MinAge, query.MaxAge);

            if (query.PageIndex == 1)
            {
                var jtk = await redis.GetAsync<JToken>(rdk);
                if (jtk != null)
                {
                    cc = (int)jtk["totalItemCount"];
                    items = jtk["page1_items"].ToObject<PcCourseItemDto2[]>();
                }
            }
            if (items == null) // find in db sql
            {
                var sql = $@"
select count(1) from (select 1 as a
from [course] c left join [Organization] o on o.id=c.orgid
where c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and c.type={CourseTypeEnum.Course.ToInt()} and c.IsInvisibleOnline=0
and o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} and o.[authentication]=1
{{0}}
)T
---
select c.no,o.id as orgid,c.id,o.name as orgname,c.title,c.subtitle,c.banner,c.price,c.origprice,o.authentication,c.sellcount,isnull(c.IsExplosions,0)as IsExplosions
from [course] c left join [Organization] o on o.id=c.orgid
where c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and c.type={CourseTypeEnum.Course.ToInt()} and c.IsInvisibleOnline=0
and o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} and o.[authentication]=1
{{0}}
order by c.sellcount desc,c.createtime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
                var sql_where = new StringBuilder();
                if (query.MinAge > 0 && query.MaxAge > 0)
                {
                    sql_where.AppendLine("and not(c.maxage<@MinAge or c.minage>@MaxAge) ");
                }

                sql = string.Format(sql, sql_where);

                var gr = await _unitOfWork.QueryMultipleAsync(sql, new DynamicParameters(query));
                cc = await gr.ReadFirstAsync<int>();
                items = gr.Read<(long, Guid), PcCourseItemDto2, PcCourseItemDto2>((lg, item) =>
                {
                    item.Banner = item.Banner.IsNullOrEmpty() ? null : item.Banner?.ToObject<string[]>()?.ElementAtOrDefault(0);
                    item.Id_s = UrlShortIdUtil.Long2Base32(lg.Item1);
                    return item;
                }, "id").AsArray();

                if (query.PageIndex == 1)
                {
                    await redis.SetAsync(rdk, new { totalItemCount = cc, page1_items = items }, 60 * 8);
                }
            }
            var pg = items.ToPagedList(query.PageSize, query.PageIndex, cc);

            // urls
            {
                foreach (var item in items)
                {
                    item.MUrl = $"{_config["BaseUrls:org-m"]}/course/detail/{item.Id_s}";
                    item.PcUrl = $"{_config["BaseUrls:org-pc"]}/course/detail/{item.Id_s}";

                    // mpqrcode
                    try
                    {
                        var cmd1 = _config.GetSection("AppSettings:CreateMpQrcode:course-detail").Get<CreateMpQrcodeCmd>();
                        cmd1.Scene = cmd1.Scene.FormatWith(item.Id_s);
                        item.MpQrcode = (await _mediator.Send(cmd1)).MpQrcode;
                    }
                    catch { /* ignore */ }
                }
            }

            return pg;
        }

        IEnumerable<int> GetSubjs()
        {
            yield return SubjectEnum.Chinese.ToInt();
            yield return SubjectEnum.Math.ToInt();
            yield return SubjectEnum.English.ToInt();
            yield return SubjectEnum.Steam.ToInt();
            yield return SubjectEnum.Draw.ToInt();
            yield return SubjectEnum.Music.ToInt();
        }

        IEnumerable<int> GetMainSubj(bool includeOther = false)
        {
            foreach (var c1 in _config.GetSection("AppSettings:pc_orgListPage_subjSide").GetChildren())
            {
                var temp = c1["item2"];
                if (!temp.IsNullOrWhiteSpace())
                {
                    var i = int.TryParse(temp, out var _i) ? _i : 0;
                    if (!includeOther && i.In(-1, 0, (int)OrgCfyEnum.Other)) continue;
                    yield return i;
                }
            }
        }
    }
}
