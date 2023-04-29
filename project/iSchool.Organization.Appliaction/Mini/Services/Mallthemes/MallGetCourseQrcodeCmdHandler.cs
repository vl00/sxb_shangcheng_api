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

namespace iSchool.Organization.Appliaction.Services
{
    public class MallGetCourseQrcodeCmdHandler : IRequestHandler<MallGetCourseQrcodeCmd, MallGetCourseQrcodeCmdResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public MallGetCourseQrcodeCmdHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<MallGetCourseQrcodeCmdResult> Handle(MallGetCourseQrcodeCmd cmd, CancellationToken cancellation)
        {
            var result = new MallGetCourseQrcodeCmdResult();
            await default(ValueTask);

            var id = Guid.TryParse(cmd.Id, out var _id) ? _id : default;
            var no = id == default ? UrlShortIdUtil.Base322Long(cmd.Id) : default;

            var course = await _mediator.Send(new CourseBaseInfoQuery { CourseId = id, No = no });

            var cmd1 = _config.GetSection("AppSettings:CreateMpQrcode:course-detail").Get<CreateMpQrcodeCmd>();
            cmd1.Scene = cmd1.Scene.FormatWith(UrlShortIdUtil.Long2Base32(course.No));
            result.MpQrcode = (await _mediator.Send(cmd1)).MpQrcode;

            return result;
        }

    }
}
