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
    public class SpecialActivityQueryHandler : IRequestHandler<SpecialActivityQuery, HdDataInfoDto[]>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IMapper mapper;

        public SpecialActivityQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis, IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.mapper = mapper;
        }

        public async Task<HdDataInfoDto[]> Handle(SpecialActivityQuery query, CancellationToken cancellation)
        {
            var rdk = CacheKeys.Hd_spcl_acti.FormatWith(query.SpecialId);
            var actis = await redis.GetAsync<Activity[]>(rdk);
            if (actis == null)
            {
                var sql = $@"
select s.id as specialid,s.no as specialid_s,s.title as SpecialName,a.*
from [Special] s left join ActivityExtend ae on ae.type={ActivityExtendType.Special.ToInt()} and ae.contentid=s.id and ae.IsValid=1
left join Activity a on a.IsValid=1 and a.status={ActivityStatus.Ok.ToInt()} and a.id=ae.activityid
where s.IsValid=1 and s.status={SpecialStatusEnum.Ok.ToInt()} and a.IsValid=1 and s.id=@SpecialId
";
                actis = (await unitOfWork.QueryAsync<Activity>(sql, new { query.SpecialId })).AsArray();

                await redis.SetAsync(rdk, actis, 60 * 20);
            }
            if (actis?.Length < 1) return null;
            return actis.Select(_ => new HdDataInfoDto { Data = _ }).ToArray();
        }

        
    }
}
