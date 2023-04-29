using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class HotSellCoursesForSchoolV2QueryHandler : IRequestHandler<HotSellCoursesForSchoolV2Query, HotSellCoursesForSchoolV2QryResult>
    {
        OrgUnitOfWork _unitOfWork;
        IMediator _mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper _mapper;
        IConfiguration _config;

        public HotSellCoursesForSchoolV2QueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IUserInfo me, IConfiguration config,
            IMapper mapper)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<HotSellCoursesForSchoolV2QryResult> Handle(HotSellCoursesForSchoolV2Query query, CancellationToken cancellation)
        {
            var result = new HotSellCoursesForSchoolV2QryResult();

            var rr = await _mediator.Send(new HotSellCoursesOrgsForSchoolsQuery { MinAge = query.MinAge, MaxAge = query.MaxAge });
            result.Time = rr.Time;
            result.HotSellCourses = rr.HotSellCourses.AsArray();

            // urls
            {
                foreach (var item in result.HotSellCourses)
                {
                    item.MUrl = $"{_config["BaseUrls:org-m"]}/course/detail/{item.Id_s}";
                    item.PcUrl = $"{_config["BaseUrls:org-pc"]}/course/detail/{item.Id_s}";

                    // mpqrcode
                    try
                    {
                        var cmd1 = _config.GetSection("AppSettings:CreateMpQrcode:course-detail").Get<CreateMpQrcodeCmd>();
                        cmd1.Scene = cmd1.Scene.FormatWith(item.Id_s);
                        item.MpQrcode = (await _mediator.Send(cmd1)).MpQrcode;
                    }
                    catch { /* ignore */ }
                }
            }

            return result;
        }

        
    }
}
