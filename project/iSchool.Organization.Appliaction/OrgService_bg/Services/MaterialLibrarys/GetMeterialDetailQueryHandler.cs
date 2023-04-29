using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
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
    public class GetMeterialDetailQueryHandler : IRequestHandler<GetMeterialDetailQuery, MeterialDetailDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public GetMeterialDetailQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<MeterialDetailDto> Handle(GetMeterialDetailQuery query, CancellationToken cancellation)
        {
            var result = new MeterialDetailDto();
            await default(ValueTask);

            var dbm = await _orgUnitOfWork.DbConnection.GetAsync<MaterialLibrary>(query.Id);
            if (dbm == null || !dbm.IsValid) return result;

            var course = await _orgUnitOfWork.DbConnection.GetAsync<iSchool.Organization.Domain.Course>(dbm.CourseId);
            if (course == null) throw new CustomResponseException("无效的商品", 400);

            result.Id = dbm.Id;
            result.CourseId = dbm.CourseId;
            result.CourseTitle = course.Title;
            result.Title = dbm.Title;
            result.Content = dbm.Content;
            result.Pictures = dbm.pictures.IsNullOrEmpty() ? new string[0] : dbm.pictures.ToObject<string[]>();
            result.Thumbnails = dbm.thumbnails.IsNullOrEmpty() ? new string[0] : dbm.thumbnails.ToObject<string[]>();
            result.Video = dbm.video;
            result.VideoCover = dbm.videoCover;
            result.Status = dbm.Status;
            result.DownloadTime = dbm.DownloadTime;

            return result;
        }

    }
}
