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
    public class OrgzBaseInfoQueryHandler : IRequestHandler<OrgzBaseInfoQuery, Domain.Organization>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IMapper mapper;

        const int cache_exp = 60 * 30;

        public OrgzBaseInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis, IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.mapper = mapper;
        }

        public async Task<Domain.Organization> Handle(OrgzBaseInfoQuery req, CancellationToken cancellation)
        {
            var orgId = req.OrgId;
            string sql = null;
            string rdkNo = null, rdk = null;
            Domain.Organization dy = null;

            if (orgId == default)
            {
                rdkNo = CacheKeys.orgidbyno.FormatWith(req.No);
                var str_Id = await redis.GetAsync<string>(rdkNo);
                if (str_Id != null)
                {
                    orgId = Guid.Parse(str_Id);
                }
            }
            if (orgId != default)
            {
                rdk = CacheKeys.OrgzBaseInfo.FormatWith(orgId);
                dy = await redis.GetAsync<Domain.Organization>(rdk);                
            }
            if (dy == null)
            {
                sql = $@"
select * from Organization o 
where 1=1 {"and o.IsValid=1 and o.status=@status".If(!req.AllowNotValid)} 
{"and o.Id=@Id".If(orgId != default)} {"and o.no=@no".If(orgId == default)}
";
                dy = await unitOfWork.QueryFirstOrDefaultAsync<Domain.Organization>(sql, new { no = req.No, Id = orgId, status = OrganizationStatusEnum.Ok.ToInt() });
                if (dy == null) throw new CustomResponseException($"无效的机构no={req.No}", 404);
                orgId = dy.Id;

                rdkNo ??= CacheKeys.orgidbyno.FormatWith(dy.No);
                rdk ??= CacheKeys.OrgzBaseInfo.FormatWith(orgId);
                using var pipe = redis.StartPipe();
                await pipe.Set(rdkNo, orgId, 60 * 60 * 1)
                    .Set(rdk, dy, cache_exp)
                    .EndPipeAsync();
            }
            if (req.No == default) req.No = dy.No;

            return dy;
        }

        
    }
}
