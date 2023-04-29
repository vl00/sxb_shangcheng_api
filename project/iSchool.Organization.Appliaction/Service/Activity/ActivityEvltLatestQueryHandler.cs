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
    public class ActivityEvltLatestQueryHandler : IRequestHandler<ActivityEvltLatestQuery, ActivityEvaluationBind>
    {
        OrgUnitOfWork orgUnitOfWork;
        IMediator mediator;
        CSRedisClient redis;
        IMapper mapper;

        public ActivityEvltLatestQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator,
            CSRedisClient redis, IMapper mapper)
        {
            this.orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
            this.mapper = mapper;
        }

        public async Task<ActivityEvaluationBind> Handle(ActivityEvltLatestQuery query, CancellationToken cancellation)
        {
            var sql = $"select top 1 * from ActivityEvaluationBind where IsValid=1 and IsLatest=1 and Evaluationid=@EvltId order by Mtime desc";
            return await orgUnitOfWork.QueryFirstOrDefaultAsync<ActivityEvaluationBind>(sql, new { query.EvltId });
        }

        
    }
}
