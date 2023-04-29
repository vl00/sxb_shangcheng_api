using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
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
    public class PcRelatedEvaluationsQueryHandler : IRequestHandler<PcRelatedEvaluationsQuery, PcRelatedEvaluationsListDto>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;
        IConfiguration config;

        public PcRelatedEvaluationsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, IUserInfo me,   
            IConfiguration config,
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this.mapper = mapper;
            this.config = config;
        }

        void Valid_query(PcRelatedEvaluationsQuery query)
        {
            if ((query?.Len ?? -1) < 1)
            {
                throw new CustomResponseException("相关评测数应该大于0");
            }

            // 进入机构详情页
            if (query.OrgId != null && query.EvltId == null && query.CourseId == null && query.Subj == null)
            {
                return;
            }
            // 进入评测详情页
            if (query.OrgId == null && query.EvltId != null)
            {
                return;
            }
            // 进入课程详情页
            if (query.OrgId == null && query.EvltId == null && query.CourseId != null)
            {
                return;
            }

            throw new CustomResponseException("相关评测参数参入错误");            
        }

        public async Task<PcRelatedEvaluationsListDto> Handle(PcRelatedEvaluationsQuery query, CancellationToken cancellation)
        {
            await default(ValueTask);
            Valid_query(query);

            // find evlts
            var rdk = query.OrgId != null ? CacheKeys.PC_OrgRelatedEvlts.FormatWith(query.OrgId) : 
                query.EvltId != null ? CacheKeys.PC_EvltRelateds.FormatWith(query.EvltId) :
                CacheKeys.PC_CourseRelatedEvlts.FormatWith(query.CourseId, query.Subj);

            var result = await redis.GetAsync<PcRelatedEvaluationsListDto>(rdk);
            if (result == null)
            {
                result = new PcRelatedEvaluationsListDto();                
                var len = query.Len;
                var subj = query.Subj;
                string sql = null;
                var items = new List<EvaluationItemDto>(len);

                if (items.Count < len && query.OrgId != null)
                {
                    sql = $@"
select top {(len - items.Count)} e.* 
from Evaluation e
left join EvaluationBind eb on eb.evaluationid=e.id and eb.isvalid=1
left join Organization o on o.IsValid=1 and o.id=eb.orgid and o.status={OrganizationStatusEnum.Ok.ToInt()}
where e.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()} {(items.Count > 0 ? $"and e.id not in ({string.Join(',', items.Select(x => $"'{x.Id}'"))})" : "")}
and o.id=@OrgId
order by e.stick desc,e.CreateTime desc
";
                    var qs = (await unitOfWork.QueryAsync<Evaluation>(sql, new { query.OrgId }))
                       .Select(x => mapper.Map<EvaluationItemDto>(x));
                    items.AddRange(qs);

                    var org_info = await mediator.Send(new OrgzBaseInfoQuery { OrgId = query.OrgId.Value });
                    // 够数 跳转选定机构
                    if (items.Count >= len)
                    {                       
                        result.OrgId = UrlShortIdUtil.Long2Base32(org_info.No);
                        result.Subj = null;
                    }
                    // 不够数 跳转选定科目,无科目选定全部
                    else
                    {
                        result.OrgId = null;
                        subj = org_info.Subjects?.ToObject<int[]>()?.ElementAtOrDefault(0) is int _subj && _subj > 0 ? _subj : (int?)null;
                        result.Subj = subj;
                    }
                }
                // 不够查相同课程 (包括评测详情页里的)
                if (items.Count < len && query.CourseId != null)
                {
                    sql = $@"
select top {(len - items.Count)} e.* --,eb.Subject as eb_subj,c.Subject as c_subj,eb.courseid
from Evaluation e
left join EvaluationBind eb on eb.evaluationid=e.id and eb.isvalid=1
left join Course c on c.id=eb.courseid and c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()}
where e.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()} {(query.EvltId != null ? $"and e.id<>@EvltId" : "")}
and c.id=@CourseId {(items.Count > 0 ? $"and e.id not in ({string.Join(',', items.Select(x => $"'{x.Id}'"))})" : "")}
order by e.stick desc,e.CreateTime desc
";
                    var qs = (await unitOfWork.QueryAsync<Evaluation>(sql, new { query.EvltId, query.CourseId }))
                        .Select(x => mapper.Map<EvaluationItemDto>(x));
                    items.AddRange(qs);

                    // 跳转选定科目, 无科目选定全部
                    // 没传入科目则取课程的
                    if (subj == null)
                    {
                        var course_info = await mediator.Send(new CourseBaseInfoQuery { CourseId = query.CourseId.Value });
                        subj = course_info?.Subject;
                    }
                    result.Subj = subj;
                }
                // 不够查相同科目
                if (items.Count < len && !subj.In(null, 0) && subj > 0)
                {
                    sql = $@"
select top {(len - items.Count)} e.* --,eb.Subject as eb_subj,c.Subject as c_subj,eb.courseid
from Evaluation e
left join EvaluationBind eb on eb.evaluationid=e.id and eb.isvalid=1
left join Course c on c.id=eb.courseid and c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()}
where e.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()} {(query.EvltId != null ? $"and e.id<>@EvltId" : "")}
{(items.Count > 0 ? $"and e.id not in ({string.Join(',', items.Select(x => $"'{x.Id}'"))})" : "")}
{(subj == SubjectEnum.Other.ToInt() ? $@"and (case when eb.courseid is null then eb.subject else c.subject end) not in({string.Join(',', GetMainSubj())})"
: "and (case when eb.courseid is null then eb.subject else c.subject end)=@subj")}
order by e.stick desc,e.CreateTime desc
";
                    var qs = (await unitOfWork.QueryAsync<Evaluation>(sql, new { subj, query.EvltId }))
                        .Select(x => mapper.Map<EvaluationItemDto>(x));
                    items.AddRange(qs);

                    result.Subj = subj;
                }
                // 不够查全部
                if (items.Count < len)
                {
                    sql = $@"
select top {(len - items.Count)} e.*
from Evaluation e
where e.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()} {(query.EvltId != null ? $"and e.id<>@EvltId" : "")}
{(items.Count > 0 ? $"and e.id not in ({string.Join(',', items.Select(x => $"'{x.Id}'"))})" : "")}
order by e.stick desc,e.CreateTime desc
";
                    var qs = (await unitOfWork.QueryAsync<Evaluation>(sql, new { query.EvltId }))
                        .Select(x => mapper.Map<EvaluationItemDto>(x));
                    items.AddRange(qs);                    
                }

                result.Evaluations = items ?? Enumerable.Empty<EvaluationItemDto>();
                result.Subj = result.Subj == null || result.Subj.Value.In(GetMainSubj().ToArray()) ? result.Subj : SubjectEnum.Other.ToInt();
                if (result.OrgId != null) result.Subj = null;

                await redis.SetAsync(rdk, result, 60 * 60);
            }
            result.Evaluations = result.Evaluations?.AsList() ?? Enumerable.Empty<EvaluationItemDto>();

            // user info
            {
                var rr = await mediator.Send(new UserSimpleInfoQuery { UserIds = result.Evaluations.Select(_ => _.AuthorId).Distinct() });
                foreach (var item in result.Evaluations)
                {
                    var r = rr.FirstOrDefault(_ => _.Id == item.AuthorId);
                    if (r == null) continue;
                    item.AuthorName = r.Nickname;
                    item.AuthorHeadImg = r.HeadImgUrl ?? config["AppSettings:UserDefaultHeadImg"];
                }
            }

            // likes
            {
                var rr1 = await mediator.Send(new EvltLikesQuery { EvltIds = result.Evaluations.Select(_ => _.Id).Distinct().ToArray() });
                foreach (var item in result.Evaluations)
                {
                    if (!rr1.Items.TryGetValue(item.Id, out var r)) continue;
                    item.LikeCount = r.Likecount;
                    item.IsLikeByMe = r.IsLikeByMe;
                }
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
