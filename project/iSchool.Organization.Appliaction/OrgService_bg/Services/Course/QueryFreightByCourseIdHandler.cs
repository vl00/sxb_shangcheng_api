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
    public class QueryFreightByCourseIdHandler : IRequestHandler<QueryFreightByCourseId, FreightItemDto[]>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public QueryFreightByCourseIdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<FreightItemDto[]> Handle(QueryFreightByCourseId query, CancellationToken cancellation)
        {
            await default(ValueTask);

            var sql = @"select cf.* from CourseFreight cf where cf.IsValid=1 and cf.CourseId=@CourseId order by cf.Type,cf.createtime ";
            var ls = await _orgUnitOfWork.QueryAsync<CourseFreight>(sql, new { query.CourseId });

            var items = ls.Select(item => 
            {
                return new FreightItemDto 
                {
                    Type = item.Type,
                    Cost = item.Cost ?? 0,
                    Names = item.Name.IsNullOrEmpty() ? null : item.Name.ToObject<string[]>(),
                    Citys = item.Citys.IsNullOrEmpty() ? null : item.Citys.ToObject<int[]>(),
                };
            });
            return items.ToArray();
        }

    }
}
