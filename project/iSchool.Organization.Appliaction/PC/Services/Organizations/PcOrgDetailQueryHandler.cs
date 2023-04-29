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
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class PcOrgDetailQueryHandler : IRequestHandler<PcOrgDetailQuery, PcOrgDetailDto>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;
        IConfiguration config;
        AppSettings appSettings;

        public PcOrgDetailQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IUserInfo me, IConfiguration config, IOptions<AppSettings> appSettings,
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this.mapper = mapper;
            this.config = config;
            this.appSettings = appSettings.Value;
        }

        public async Task<PcOrgDetailDto> Handle(PcOrgDetailQuery query, CancellationToken cancellation)
        {
            var result = new PcOrgDetailDto();
            await default(ValueTask);
            result.Me = me.IsAuthenticated ? me : null;

            // 基本信息
            var info = await mediator.Send(new OrgzBaseInfoQuery { No = query.No, OrgId = query.OrgId });
            result.OrgInfo = mapper.Map<PcOrgItemDto>(info);
            result.OrgBaseMap = appSettings.OrgBaseMap;
            result.Intro = info.Intro;

            // for CourceCount + EvaluationCount
            var dict = await mediator.Send(new PcGetOrgsCountsQuery { OrgIds = new[] { info.Id } });
            if (dict.TryGetValue(info.Id, out var m))
            {
                result.OrgInfo.CourceCount = m.CourceCount;
                result.OrgInfo.EvaluationCount = m.EvaluationCount;
                result.OrgInfo.GoodsCount = m.GoodsCount;
            }
            else
            {
                result.OrgInfo.CourceCount = 0;
                result.OrgInfo.EvaluationCount = 0;
                result.OrgInfo.GoodsCount = 0;
            }

            // 相关评测s
            result.RecommendEvaluations = await mediator.Send(new PcRelatedEvaluationsQuery { Len = 6, OrgId = info.Id });

            // 机构(相关)课程s
            result.RelatedCourses = new PcRelatedCoursesListDto();
            {
                var rr = await mediator.Send(new PcOrgSubjRelatedCoursesQuery { Len = 3, OrgId = info.Id });
                result.RelatedCourses.Courses = rr.Items;
                result.RelatedCourses.Subj = rr.Subj;
                result.RelatedCourses.OrgId = rr.IsCurrOrgCourses ? result.Id_s : null;
            }

            return result;
        }

    }
}
