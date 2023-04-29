using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
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
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.ViewModels;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services
{
    public class QueryFreightCitys4CourseHandler : IRequestHandler<QueryFreightCitys4Course, (int Code, string Name)[]>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public QueryFreightCitys4CourseHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<(int Code, string Name)[]> Handle(QueryFreightCitys4Course query, CancellationToken cancellation)
        {
            await default(ValueTask);

            var sql = @"select id,name from cityarea where IsValid=1 and depth=1";
            var ls = await _orgUnitOfWork.QueryAsync<(int, string)>(sql, new { });

            return ls.AsArray();
        }

    }
}
