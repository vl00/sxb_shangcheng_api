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
    public class GetCourseFxSimpleInfoQueryHandler : IRequestHandler<GetCourseFxSimpleInfoQuery, CourseDrpInfo>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;        

        public GetCourseFxSimpleInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator,
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
        }

        public async Task<CourseDrpInfo> Handle(GetCourseFxSimpleInfoQuery query, CancellationToken cancellation)
        {
            var rdkey = CacheKeys.CourseDrpfxInfo.FormatWith(query.CourseId);
            var result = await _redis.GetAsync<CourseDrpInfo>(rdkey);
            if (result == null)
            {
                var sql = "select top 1 * from CourseDrpInfo where IsValid=1 and CourseId=@CourseId";
                result = await _orgUnitOfWork.QueryFirstOrDefaultAsync<CourseDrpInfo>(sql, new { query.CourseId });

                await _redis.SetAsync(rdkey, result ??= new CourseDrpInfo(), 60 * 60 * 1);
            }
            return result.Id == default ? null : result;
        }

        
    }
}
