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
    public class GetCourseMedias4MeterialQueryHandler : IRequestHandler<GetCourseMedias4MeterialQuery, GetCourseMedias4MeterialQyResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public GetCourseMedias4MeterialQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<GetCourseMedias4MeterialQyResult> Handle(GetCourseMedias4MeterialQuery query, CancellationToken cancellation)
        {
            var result = new GetCourseMedias4MeterialQyResult();
            await default(ValueTask);

            var sql = "select * from Course c where c.IsValid=1 and c.id=@CourseId";
            var course = await _orgUnitOfWork.QueryFirstOrDefaultAsync<iSchool.Organization.Domain.Course>(sql, new { query.CourseId });
            if (course == null) throw new CustomResponseException("无效的商品", 404);

            result.VideoUrl = course.Videos.IsNullOrEmpty() ? null : course.Videos.ToObject<string[]>().FirstOrDefault();
            result.VideoCoverUrl = course.Videocovers.IsNullOrEmpty() ? null : course.Videocovers.ToObject<string[]>().FirstOrDefault();
            result.Banners = course.Banner.IsNullOrEmpty() ? new string[0] : course.Banner.ToObject<string[]>();

            return result;
        }

    }
}
