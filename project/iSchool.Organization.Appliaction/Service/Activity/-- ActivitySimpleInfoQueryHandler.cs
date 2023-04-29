using AutoMapper;
using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    [Obsolete("旧活动")]
    public class ActivitySimpleInfoQueryHandler : IRequestHandler<ActivitySimpleInfoQuery, ActivitySimpleInfoDto>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IMapper mapper;

        public ActivitySimpleInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis, IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.mapper = mapper;
        }

        public async Task<ActivitySimpleInfoDto> Handle(ActivitySimpleInfoQuery req, CancellationToken cancellation)
        {
            var rdk = CacheKeys.ActivitySimpleInfo.FormatWith(req.Id);
            var r = await redis.GetAsync<ActivitySimpleInfoDto>(rdk);
            if (r == null)
            {
                var sql = @"
select a.*,s.id as SpecialId,s.title as SpecialName,s.no as SpecialId_s
from Activity a 
left join Special s on s.activity=a.id and s.IsValid=1
where a.IsValid=1 and a.id=@Id
";
                r = unitOfWork.DbConnection.QueryFirstOrDefault<ActivitySimpleInfoDto>(sql, new { req.Id });
                if (r != null) r.SpecialId_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(r.SpecialId_s));
                var s = r == null ? "{}" : r.ToJsonString(true);
                await redis.SetAsync(rdk, s, 60 * 60 * 24 * 1);
            }
            return r?.Id != req.Id ? null : r;
        }

        
    }
}
