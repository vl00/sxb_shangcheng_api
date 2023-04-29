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
    public class PcEvltDetailQueryHandler : IRequestHandler<PcEvltDetailQuery, PcEvltDetailDto>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;
        IConfiguration config;

        public PcEvltDetailQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
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

        public async Task<PcEvltDetailDto> Handle(PcEvltDetailQuery query, CancellationToken cancellation)
        {
            var result = new PcEvltDetailDto();
            var evltId = result.Id;
            await default(ValueTask);

            result.Me = me.IsAuthenticated ? me : null;

            // find base
            var rr = await mediator.Send(new EvltDetailQuery { No = query.No, EvltId = query.EvltId });
            mapper.Map(rr, result);
            evltId = result.Id;

            // 科目
            if (result.CoursePart.Subj0 == null || !result.CoursePart.Subj0.Value.In(GetMainSubj().ToArray()))
            {
                result.CoursePart.Subj = SubjectEnum.Other.ToInt();
            }

            // AuthorInfo
            {
                result.AuthorInfo = new PcEvltDetailDto_AuthorInfo();
                result.AuthorInfo.Id = result.AuthorId;
                result.AuthorInfo.Name = result.AuthorName;
                result.AuthorInfo.HeadImg = result.AuthorHeadImg;

                var rdk = CacheKeys.PC_EvltAuthorCounts.FormatWith(("evltId", evltId), ("userid", result.AuthorInfo.Id));
                var j = await redis.GetAsync<JToken>(rdk);
                if (j != null)
                {
                    result.AuthorInfo.EvaluationCount = (int)j["EvaluationCount"];
                    result.AuthorInfo.LikeCount = (int)j["LikeCount"];
                }
                else
                {
                    var sql = $@"
select count(1) as Item1,sum(likes+isnull(shamlikes,0)) as Item2
from Evaluation where IsValid=1 and status={EvaluationStatusEnum.Ok.ToInt()} and userid=@Id
";
                    var (ec, lc) = await unitOfWork.QueryFirstOrDefaultAsync<(int, int)>(sql, new { result.AuthorInfo.Id });
                    result.AuthorInfo.EvaluationCount = ec;
                    result.AuthorInfo.LikeCount = lc;

                    await redis.SetAsync(rdk, new { result.AuthorInfo.EvaluationCount, result.AuthorInfo.LikeCount }, 60 * 5);
                }
            }

            // RelatedEvaluations
            var subj = result.CoursePart?.Subject == null ? (int?)null : EnumUtil.GetDescs<SubjectEnum>().Where(_ => _.Desc == result.CoursePart.Subject).Select(_ => _.Value.ToInt()).FirstOrDefault();
            result.RelatedEvaluations = await mediator.Send(new PcRelatedEvaluationsQuery
            {
                Len = 8,
                EvltId = evltId,
                CourseId = result.CoursePart?.CourseId,
                Subj = subj,
            });

            // Comments
            do
            {
                result.Comments = await mediator.Send(new PcEvltCommentsQuery
                {
                    EvltId = result.Id,
                    PageIndex = 1,
                    PageSize = 10,
                    Naf = result.Now,
                });
                if (result.Comments?.CurrentPageItems == null) break;
                foreach (var m in result.Comments.CurrentPageItems)
                {
                    m.Now = result.Now;
                }
            }
            while (false);
            
            return result;
        }

        IEnumerable<int> GetMainSubj()
        {
            foreach (var c1 in config.GetSection("AppSettings:pc_elvtMainPage_subjSide").GetChildren())
            {
                var temp = c1["item2"];
                if (!temp.IsNullOrWhiteSpace())
                {
                    var i = int.TryParse(temp, out var _i) ? _i : 0;
                    if (i.In(0, (int)SubjectEnum.Other)) continue;
                    yield return i;
                }
            }
        }
    }
}
