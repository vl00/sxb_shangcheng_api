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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class MiniRwInviteActivityCoursesQueryHandler : IRequestHandler<MiniRwInviteActivityCoursesQuery, MiniRwInviteActivityCoursesQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient redis;        
        IMapper mapper;
        IConfiguration config;

        public MiniRwInviteActivityCoursesQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.redis = redis;            
            this.mapper = mapper;
            this.config = config;
        }

        public async Task<MiniRwInviteActivityCoursesQryResult> Handle(MiniRwInviteActivityCoursesQuery query, CancellationToken cancellation)
        {
            var result = new MiniRwInviteActivityCoursesQryResult();
            await default(ValueTask);

            var rdkey = (query.City ?? 0) <= 0 ? CacheKeys.RwInviteActivity_InvisibleOnlineCourses
                : CacheKeys.RwInviteActivity_InvisibleOnlineCoursesWithCity.FormatWith(query.City);

            var courseIds = await redis.SMembersAsync<Guid>(rdkey);
            if (courseIds?.Length < 1)
            {
                result.Courses = new MiniRwInviteActivityCourseDto[0];
                return result;
            }

            var sql = $@"
select c.no,isnull(c.banner_s,c.banner) as _banners,c.orgid,o.no as orgno,
c.id,c.type,c.title,c.subtitle,c.price,c.origprice,
e.type as ExchangeType,e.point as ExchangePoint,e.starttime as ExchangeStartTime,e.endtime as ExchangeEndTime,e.Keywords,
o.name,o.logo,o.[desc],o.subdesc,o.authentication
from Course c
left join Organization o on o.id=c.orgid
left join CourseExchange e on e.Courseid=c.id and e.IsValid=1
where c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and c.IsInvisibleOnline=1 
and o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()}
and c.id in @courseIds
";
            var qs = await _orgUnitOfWork.QueryAsync<(long, string, Guid, long), MiniRwInviteActivityCourseDto, string, PcOrgItemDto0, MiniRwInviteActivityCourseDto>(
                sql: sql,
                splitOn: "id,Keywords,name",
                param: new { courseIds },
                map: (itm, dto, keywords, org) =>
                {
                    dto.Id_s = UrlShortIdUtil.Long2Base32(itm.Item1);
                    dto.Banner = itm.Item2.IsNullOrEmpty() ? null : itm.Item2.ToObject<string[]>()?.FirstOrDefault();
                    org.Id_s = UrlShortIdUtil.Long2Base32(itm.Item4);
                    org.Id = itm.Item3;
                    dto.OrgInfo = org;
                    dto.ExchangeKeywords = keywords.IsNullOrEmpty() ? new string[0] : keywords.ToObject<string[]>();
                    return dto;
                }
            );
            result.Courses = qs;

            return result;
        }

    }
}
