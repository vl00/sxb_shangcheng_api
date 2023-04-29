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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class PcCourseDetailQueryHandler : IRequestHandler<PcCourseDetailQuery, PcCourseDetailDto>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;
        IConfiguration config;

        public PcCourseDetailQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IUserInfo me, IConfiguration config,
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this.mapper = mapper;
            this.config = config;
        }

        public async Task<PcCourseDetailDto> Handle(PcCourseDetailQuery query, CancellationToken cancellation)
        {
            var result = new PcCourseDetailDto();
            await default(ValueTask);

            result.Me = me.IsAuthenticated ? me : null;
            var subjs = GetMainSubj().ToArray();

            var info = await mediator.Send(new CourseBaseInfoQuery { No = query.No, CourseId = query.CourseId });
            if (info.Status != CourseStatusEnum.Ok.ToInt()) throw new CustomResponseException("已下架", 404);
            mapper.Map(info, result);
            if (!result.Subject.In(subjs)) result.Subj = SubjectEnum.Other.ToInt();
            else result.Subj = result.Subject;

            // 机构信息
            var org_info = await mediator.Send(new OrgzBaseInfoQuery { OrgId = info.Orgid });
            {
                result.OrgInfo = mapper.Map<PcOrgItemDto>(org_info);

                // for CourceCount + EvaluationCount
                var dict = await mediator.Send(new PcGetOrgsCountsQuery { OrgIds = new[] { org_info.Id } });
                if (dict.TryGetValue(org_info.Id, out var m))
                {
                    result.OrgInfo.CourceCount = m.CourceCount;
                    result.OrgInfo.EvaluationCount = m.EvaluationCount;
                }
                else
                {
                    result.OrgInfo.CourceCount = 0;
                    result.OrgInfo.EvaluationCount = 0;
                }
            }

            // 相关评测s
            result.RelatedEvaluations = await mediator.Send(new PcRelatedEvaluationsQuery { Len = 6, CourseId = info.Id, Subj = info.Subject });

            // 机构课程or相关课程
            {
                var rr = await mediator.Send(new PcOrgSubjRelatedCoursesQuery { Len = 3, CourseId = info.Id, OrgId = info.Orgid });
                result.RelatedCourses ??= new PcRelatedCoursesListDto();
                result.RelatedCourses.Courses = rr.Items;
                result.RelatedCourses.Subj = rr.Subj;
                result.RelatedCourses.OrgId = rr.IsCurrOrgCourses ? result.OrgInfo.Id_s : null;
            }

            if (query.AllowRecordPV)
            {
                // 增加PV
                AsyncUtils.StartNew(new PVisitEvent { CttId = result.Id, UserId = me.UserId, Now = DateTime.Now, CttType = PVisitCttTypeEnum.Course });
            }

            return result;
        }

        IEnumerable<int> GetMainSubj()
        {
            foreach (var c1 in config.GetSection("AppSettings:pc_courseListPage_subjSide").GetChildren())
            {
                var temp = c1["item2"];
                if (!temp.IsNullOrWhiteSpace())
                {
                    var i = int.TryParse(temp, out var _i) ? _i : 0;
                    if (i.In(-1, 0, (int)SubjectEnum.Other)) continue;
                    yield return i;
                }
            }
        }
    }
}
