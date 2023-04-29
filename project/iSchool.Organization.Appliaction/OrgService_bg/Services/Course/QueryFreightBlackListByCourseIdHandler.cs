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
    public class QueryFreightBlackListByCourseIdHandler : IRequestHandler<QueryFreightBlackListByCourseId, FreightBlackListDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public QueryFreightBlackListByCourseIdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<FreightBlackListDto> Handle(QueryFreightBlackListByCourseId query, CancellationToken cancellation)
        {
            await default(ValueTask);

            var sql = @"select c.* from Course c where c.IsValid=1 and c.id=@CourseId ";
            var course = await _orgUnitOfWork.QueryFirstOrDefaultAsync<Domain.Course>(sql, new { query.CourseId });
            if (course == null) throw new CustomResponseException("商品不存在");

            var backlist = (course.BlackList ?? "[]").ToObject<int[]>();
            if (backlist == null || backlist.Length == 0)
            {
                return new FreightBlackListDto { Citys = new int[0], Names = new string[0] };
            }
            if (backlist.Length == 1 && backlist[0] == 0)
            {
                return new FreightBlackListDto { Citys = backlist, Names = new[] { "全国" } };
            }

            sql = "select id,name from cityarea where IsValid=1 and id in @backlist";
            var cityareas = (await _orgUnitOfWork.QueryAsync<(int, string)>(sql, new { backlist })).OrderBy(_ => _.Item1);
            var dto = new FreightBlackListDto();
            dto.Citys = cityareas.Select(_ => _.Item1).ToArray();
            dto.Names = cityareas.Select(_ => _.Item2).ToArray();

            return dto;
        }

    }
}
