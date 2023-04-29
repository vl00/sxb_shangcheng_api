using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class PcOrgSubjRelatedCoursesQueryHandler : IRequestHandler<PcOrgSubjRelatedCoursesQuery, PcOrgSubjRelatedCoursesQueryResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;
        IConfiguration config;

        public PcOrgSubjRelatedCoursesQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
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

        public async Task<PcOrgSubjRelatedCoursesQueryResult> Handle(PcOrgSubjRelatedCoursesQuery query, CancellationToken cancellation)
        {
            var subjs = GetMainSubj().ToArray();
            var len = query.Len;
            await default(ValueTask);

            var rdk = query.CourseId != null ? CacheKeys.PC_CourseRelatedCourses.FormatWith(query.CourseId) :
                CacheKeys.PC_OrgRelatedCourses.FormatWith(query.OrgId);

            var result = await redis.GetAsync<PcOrgSubjRelatedCoursesQueryResult>(rdk);
            if (result == null) 
            {
                result = new PcOrgSubjRelatedCoursesQueryResult();
                string sql = null;
                IEnumerable<(PcCourseItemDto, string)> dys = null;
                int? subj = null;

                if (subj == null && query.CourseId is Guid courseId)
                {
                    var info = await mediator.Send(new CourseBaseInfoQuery { CourseId = courseId });
                    subj = info?.Subject;
                }

                // 相同机构下
                {
                    result.IsCurrOrgCourses = true;

                    sql = $@"
select top {len} c.id,c.no,org.name,c.title,c.subtitle,c.price,c.origprice,c.stock,org.authentication,c.subject,c.banner as banner_0 
from Course c left join Organization org on org.IsValid=1 and c.orgid=org.id and org.status={OrganizationStatusEnum.Ok.ToInt()}
where c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and c.type={CourseTypeEnum.Course.ToInt()} and c.IsInvisibleOnline=0
{"and c.id<>@CourseId".If(query.CourseId != null)} and org.id=@OrgId
order by org.authentication desc, c.CreateTime desc
";
                    dys = await unitOfWork.QueryAsync<PcCourseItemDto, string, (PcCourseItemDto, string)>(sql, param: query,
                        splitOn: "banner_0", map: (_0, _1) => (_0, _1));
                }
                // 相同科目下
                if (dys?.Any() != true)
                {
                    result.IsCurrOrgCourses = false;

                    if (subj == null && query.OrgId is Guid orgId)
                    {
                        var info = await mediator.Send(new OrgzBaseInfoQuery { OrgId = orgId });
                        subj = info?.Subjects?.ToObject<int?[]>()?.ElementAtOrDefault(0);
                    }
                    if (subj.In(null, 0)) subj = null;

                    sql = $@"
select top {len} c.id,c.no,org.name,c.title,c.subtitle,c.price,c.origprice,c.stock,org.authentication,c.subject,c.banner as banner_0 
from Course c left join Organization org on org.IsValid=1 and c.orgid=org.id and org.status={OrganizationStatusEnum.Ok.ToInt()}
where c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and c.type={CourseTypeEnum.Course.ToInt()} and c.IsInvisibleOnline=0
{(subj.In(-1, SubjectEnum.Other.ToInt()) ? $"and c.subject not in({string.Join(',', subjs)})" :
subj != null ? "and c.subject=@subj" : "")}
{"and c.id<>@CourseId".If(query.CourseId != null)}
order by org.authentication desc, c.CreateTime desc
";
                    dys = await unitOfWork.QueryAsync<PcCourseItemDto, string, (PcCourseItemDto, string)>(sql,
                        param: new { subj, query.OrgId, query.CourseId },
                        splitOn: "banner_0", map: (_0, _1) => (_0, _1));
                    subj = null;
                }
                result.Items = dys.Select(x =>
                {
                    x.Item1.Banner = x.Item2.ToObject<List<string>>();
                    x.Item1.Id_s = x.Item1.No == null ? null : UrlShortIdUtil.Long2Base32(x.Item1.No.Value);
                    x.Item1.No = null;
                    return x.Item1;
                }).ToArray();
                result.Subj = subj ?? result.Items?.ElementAtOrDefault(0)?.Subject ?? SubjectEnum.Other.ToInt();

                await redis.SetAsync(rdk, result, 60 * 90);
            }
            
            if (!result.Subj.In(subjs)) result.Subj = SubjectEnum.Other.ToInt();
            return result;
        }

        IEnumerable<int> GetMainSubj()
        {
            foreach (var c1 in config.GetSection("AppSettings:pc_courseListPage_subjSide").GetChildren())
            {
                var temp = c1["item2"];
                if (!temp.IsNullOrWhiteSpace())
                {
                    var i = int.TryParse(temp, out var _i) ? _i : 0;
                    if (i.In(-1, 0, (int)SubjectEnum.Other)) continue;
                    yield return i;
                }
            }
        }
    }
}
