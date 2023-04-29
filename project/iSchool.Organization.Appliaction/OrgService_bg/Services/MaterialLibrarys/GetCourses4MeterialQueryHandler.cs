using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services
{
    public class GetCourses4MeterialQueryHandler : IRequestHandler<GetCourses4MeterialQuery, GetCourses4MeterialQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public GetCourses4MeterialQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<GetCourses4MeterialQryResult> Handle(GetCourses4MeterialQuery query, CancellationToken cancellation)
        {
            var result = new GetCourses4MeterialQryResult();
            await default(ValueTask);

            var sql = "select top 10 c.id,c.title from Course c where c.IsValid=1 and c.title like @Txt order by c.createtime desc";
            var ls = await _orgUnitOfWork.QueryAsync<(Guid, string)>(sql, new { Txt = $"%{query.Txt}%" });
            result.Courses = ls.AsArray();

            return result;
        }

    }
}
