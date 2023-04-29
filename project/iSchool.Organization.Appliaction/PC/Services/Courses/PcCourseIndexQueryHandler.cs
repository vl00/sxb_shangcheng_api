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
    public class PcCourseIndexQueryHandler : IRequestHandler<PcCourseIndexQuery, PcCourseIndexQueryResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;
        IConfiguration config;

        public PcCourseIndexQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IUserInfo me, IConfiguration config,
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this.mapper = mapper;
            this.config = config;
        }

        public async Task<PcCourseIndexQueryResult> Handle(PcCourseIndexQuery query, CancellationToken cancellation)
        {
            var result = new PcCourseIndexQueryResult();
            await default(ValueTask);
            var subjs = GetMainSubj(true).Select(_ => new SelectItemsKeyValues { Key = _, Value = EnumUtil.GetDesc((SubjectEnum)_) }).AsList();

            result.Me = me.IsAuthenticated ? me : null;
            result.Subjs = subjs;

            if (query.OrgNo != null)
            {                
                var org_info = await mediator.Send(new OrgzBaseInfoQuery { No = query.OrgNo.Value });
                result.OrgInfo = mapper.Map<PcOrgItemDto>(org_info);

                // 需要去掉页面科目栏里没该机构课程的科目
                var org_subjs = org_info.Subjects?.ToObject<int[]>();
                if ((org_subjs?.Length ?? 0) == 0) subjs.RemoveAll(_ => _.Key != SubjectEnum.Other.ToInt());
                else subjs.RemoveAll(_ => _.Key != SubjectEnum.Other.ToInt() && !_.Key.In(org_subjs));
            }

            result.PageInfo = await GetPagedList(query, result.OrgInfo, subjs);

            return result;
        }

        async Task<PagedList<PcCourseItemDto>> GetPagedList(PcCourseIndexQuery query, PcOrgItemDto orgInfo, IEnumerable<SelectItemsKeyValues> subjs)
        {
            IEnumerable<PcCourseItemDto> items = null;
            int cc = 0;
            await default(ValueTask);
            subjs = subjs.Where(_ => _.Key != SubjectEnum.Other.ToInt());

            var rdk = CacheKeys.PC_CourseIndexLs.FormatWith(query.PageSize, query.Subj, 
                query.Authentication switch { true => 1, false => 2, _ => "" }, 
                orgInfo?.Id);

            if (query.PageIndex == 1)
            {
                var jtk = await redis.GetAsync<JToken>(rdk);
                if (jtk != null)
                {
                    cc = (int)jtk["totalItemCount"];
                    items = jtk["page1_items"].ToObject<PcCourseItemDto[]>();
                }
            }
            if (items == null) // find in db sql
            {
                var sql = $@"
select c.id,c.title,c.subtitle,c.orgid,o.name,o.authentication,c.price,c.origprice,c.stock,c.subject,
c.banner as banner_json,c.no from [dbo].[Course] c
left join [dbo].[Organization] o on o.id=c.orgid and o.IsValid=1 
where c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and o.status={OrganizationStatusEnum.Ok.ToInt()} and c.IsInvisibleOnline=0
and c.type={CourseTypeEnum.Course.ToInt()} {{0}}
{(query.Authentication == null ? "" : $"and o.authentication={(query.Authentication == true ? 1 : 0)}")}
{"and o.id=@orgid".If(orgInfo != null)}
";
                var sql_where = "";
                if (query.Subj != null)
                {
                    var is_other = query.Subj.Value.In(-1, SubjectEnum.Other.ToInt()) 
                        || !query.Subj.Value.In(subjs.Select(_ => _.Key).ToArray());
                    sql_where += !is_other ? "and c.subject=@Subj" :
                        !subjs.Any() ? "" :
                        $@"and (c.subject is null or c.subject not in ({string.Join(',', subjs.Select(_ => _.Key))}) ) ";
                }

                sql = string.Format(sql, sql_where);
                sql = $@"
select count(1) from ({sql}) T
;
{sql}
order by o.authentication desc,c.CreateTime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
                var gr = await unitOfWork.QueryMultipleAsync(sql, new { query.PageIndex, query.PageSize, query.Subj, orgid = orgInfo?.Id });
                cc = await gr.ReadFirstAsync<int>();
                items = gr.Read<PcCourseItemDto, string, long, PcCourseItemDto>((x, json, no) =>
                {
                    x.Banner = json?.ToObject<List<string>>() ?? new List<string>();
                    x.Id_s = UrlShortIdUtil.Long2Base32(no);
                    return x;
                }, "banner_json,no").AsArray();

                if (query.PageIndex == 1)
                {
                    await redis.SetAsync(rdk, new { totalItemCount = cc, page1_items = items }, 60 * 60);
                }
            }
            var pg = items.ToPagedList(query.PageSize, query.PageIndex, cc);

            return pg;
        }

        IEnumerable<int> GetMainSubj(bool includeOther = false)
        {
            foreach (var c1 in config.GetSection("AppSettings:pc_courseListPage_subjSide").GetChildren())
            {
                var temp = c1["item2"];
                if (!temp.IsNullOrWhiteSpace())
                {
                    var i = int.TryParse(temp, out var _i) ? _i : 0;
                    if (!includeOther && i.In(-1, 0, (int)SubjectEnum.Other)) continue;
                    yield return i;
                }
            }
        }

    }
}
