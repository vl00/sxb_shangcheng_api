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
    public class GetSkuFxSimpleInfoQueryHandler : IRequestHandler<GetSkuFxSimpleInfoQuery, CourseGoodDrpInfo>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;        
        CSRedisClient _redis;        

        public GetSkuFxSimpleInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator,
            CSRedisClient redis)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
        }

        public async Task<CourseGoodDrpInfo> Handle(GetSkuFxSimpleInfoQuery query, CancellationToken cancellation)
        {
            var rdkey = CacheKeys.SkuDrpfxInfo.FormatWith(query.SkuId);
            var result = await _redis.GetAsync<CourseGoodDrpInfo>(rdkey);
            if (result == null)
            {
                var sql = "select top 1 * from CourseGoodDrpInfo where IsValid=1 and GoodId=@SkuId";
                result = await _orgUnitOfWork.QueryFirstOrDefaultAsync<CourseGoodDrpInfo>(sql, new { query.SkuId });

                await _redis.SetAsync(rdkey, result ??= new CourseGoodDrpInfo(), 60 * 60 * 1);
            }
            return result.Id == default ? null : result;
        }

        
    }
}
