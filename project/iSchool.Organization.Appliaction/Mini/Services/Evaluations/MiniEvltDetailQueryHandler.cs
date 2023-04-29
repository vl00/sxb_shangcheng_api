using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Common;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class MiniEvltDetailQueryHandler : IRequestHandler<MiniEvltDetailQuery, MiniEvltDetailDto>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        private readonly IUserInfo me;
        private readonly CSRedisClient redis;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        const int cache_exp = 60 * 30;

        public MiniEvltDetailQueryHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis, IMapper mapper, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;
            this._mapper = mapper;
            this._config = config;
            this._httpContextAccessor = httpContextAccessor;
        }

        public async Task<MiniEvltDetailDto> Handle(MiniEvltDetailQuery req, CancellationToken cancellation)
        {
            var lsT = new List<Task>();
            var dto = await GetInfo0(new MiniEvltDetailDto(), req);
            dto.Now = DateTime.Now;

            if (me.IsAuthenticated && me.UserId == dto.AuthorId)
            {
                dto.IsSelf = true;
            }
            lsT.Add(GetAuthor(dto));

            if (req.AllowRecordPV)
            {
                // 增加PV
                AsyncUtils.StartNew(new PVisitEvent { CttId = dto.Id, UserId = me.UserId, Now = dto.Now, CttType = PVisitCttTypeEnum.Evaluation });
            }
            await GetContents(dto);

            
            await GetRelateds(req, dto);                        
            await GetMiniSharedCount(dto);
            //await GetComments(dto);

            await Task.WhenAll(lsT).ConfigureAwait(false);
            return dto;
        }

        async Task GetAuthor(MiniEvltDetailDto dto)
        {
            var au = await _mediator.Send(new UserSimpleInfoQuery
            {
                UserIds = new[] { dto.AuthorId }
            });
            dto.AuthorName = au.FirstOrDefault()?.Nickname;
            dto.AuthorHeadImg = au.FirstOrDefault()?.HeadImgUrl;
        }

        /// <summary>基本信息</summary>
        async Task<MiniEvltDetailDto> GetInfo0(MiniEvltDetailDto dto, MiniEvltDetailQuery req)
        {
            var evltId = Guid.Empty;           
            var baseInfo = await _mediator.Send(new GetEvltBaseInfoQuery { No = req.No, EvltId = req.Id });
            _mapper.Map(baseInfo, dto);
            evltId = dto.Id;
            if (req.No == default) req.No = baseInfo.No;

            // likecount + IsLikeByMe
            var likes = await _mediator.Send(new EvltLikesQuery { EvltIds = new[] { dto.Id } });
            if (likes.Items.TryGetValue(dto.Id, out var lk))
            {
                dto.LikeCount = lk.Likecount;
                dto.IsLikeByMe = lk.IsLikeByMe;
            }

            return dto;
        }

        /// <summary>内容s</summary>
        async Task GetContents(MiniEvltDetailDto dto)
        {
            var rdk = CacheKeys.Evlt.FormatWith(dto.Id);
            var contents = await redis.HGetAsync<EvaluationContentDto[]>(rdk, "contents");

            if (contents == null)
            {
                var sql = $@"
select item.* from Evaluation evlt 
left join EvaluationItem item on evlt.id=item.evaluationid and item.IsValid=1
where evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()} and evlt.id=@Id and item.type{(dto.Mode == 1 ? "=" : ">")}0
order by item.type
";
                var items = await _orgUnitOfWork.QueryAsync<EvaluationItem>(sql, new { dto.Id });
                contents = items.Select(x => _mapper.Map<EvaluationContentDto>(x)).ToArray();

                _ = redis.HSetAsync(rdk, "contents", contents);
            }

            var ctts = string.Join('\n', contents.Select(_ => _?.Content ?? ""));
            dto.Content = ctts;
            {
                dto.Imgs = contents.Select(_ => _?.Pictures).Where(_ => _ != null).SelectMany(_ => _);
                dto.Imgs_s = contents.Select(_ => _?.Thumbnails).Where(_ => _ != null).SelectMany(_ => _);
                dto.VideoUrl = contents.Select(_ => _?.VideoUrl).FirstOrDefault();
                dto.VideoCoverUrl = contents.Select(_ => _?.VideoCoverUrl).FirstOrDefault();
            }
            ctts = HtmlHelper.NoHTML(ctts);
            dto.SharedContent = ctts.Length > 50 ? ctts[0..50] : ctts;
            dto.Tdk_d = ctts.Length > 160 ? ctts[0..160] : ctts;
        }

        /// <summary>分享数</summary>
        async Task GetMiniSharedCount(MiniEvltDetailDto dto)
        {
            var items = await _mediator.Send(new GetEvltMiniSharedCountsQueryArgs(dto.Id));
            if (!items.TryGetOne(out var item, (_) => _.EvltId == dto.Id)) return;
            dto.SharedCount = item.SharedCount;
        }

        /// <summary>关联主体</summary>
        async Task GetRelateds(MiniEvltDetailQuery req, MiniEvltDetailDto dto)
        {
            var agentType = 0;
            //小程序，IOS，App屏蔽课程, 只显示好物
            if (req.AllowIosNodisplay == 1 && UserAgentUtils.IsIos(_httpContextAccessor.HttpContext))
            {
                agentType = 1;
            }
            var item = await redis.GetAsync<JToken>(CacheKeys.MiniEvltRelateds.FormatWith(dto.Id, agentType));
            if (item != null)
            {
                dto.RelatedMode = (int?)item["relatedMode"] ?? 0;
                dto.RelatedCourses = item["relatedCourses"]?.ToObject<MiniEvltRelatedCourseDto[]>();
                dto.RelatedOrgs = item["relatedOrgs"]?.ToObject<MiniEvltRelatedOrgDto[]>();
            }
            else
            {
                var sql = $@"
select eb.* from Evaluation e 
left join EvaluationBind eb on e.id=eb.Evaluationid
where e.id=@Id and eb.IsValid=1
";
                var dbm_EvaluationBinds = await _orgUnitOfWork.QueryAsync<EvaluationBind>(sql, new { dto.Id });
                if (!dbm_EvaluationBinds.Any())
                {
                    dto.RelatedMode = (int)EvltRelatedModeEnum.Other;
                    dto.RelatedCourses = null;
                    dto.RelatedOrgs = null;
                }
                else if (dbm_EvaluationBinds.Any(_ => _.Courseid != null))
                {
                    // 不用考虑下架
                    sql = $@"
select e.id as Evaluationid,c.no,c.id,c.title,c.banner,c.price from Evaluation e 
left join EvaluationBind eb on e.id=eb.Evaluationid and eb.IsValid=1
left join Course c on eb.Courseid=c.id and c.IsValid=1 
where e.id=@Id {(agentType == 1 ? "and c.type=2" : "")}
";
                    dto.RelatedMode = (int)EvltRelatedModeEnum.Course;
                    dto.RelatedCourses = (await _orgUnitOfWork.QueryAsync<Guid, long?, MiniEvltRelatedCourseDto, MiniEvltRelatedCourseDto>(sql,
                        param: new { dto.Id },
                        splitOn: "Evaluationid,no,id",
                        map: (_, no, item) =>
                        {
                            if (no == null) return null;
                            item.Id_s = UrlShortIdUtil.Long2Base32(no.Value);
                            item.Banner = string.IsNullOrEmpty(item.Banner) ? null : item.Banner.ToObject<string[]>()?.FirstOrDefault();
                            return item;
                        }
                    )).Where(_ => _ != null).AsArray();
                    dto.RelatedOrgs = null;
                }
                else if (dbm_EvaluationBinds.Any(_ => _.Orgid != null))
                {
                    // 不用考虑下架
                    sql = $@"
select e.id as Evaluationid,o.no,o.id,o.name,o.logo,o.[desc],o.subdesc,o.[authentication]
from Evaluation e 
left join EvaluationBind eb on e.id=eb.Evaluationid and eb.IsValid=1
left join Organization o on eb.orgid=o.id and o.IsValid=1 
where e.id=@Id 
";
                    dto.RelatedMode = (int)EvltRelatedModeEnum.Org;
                    dto.RelatedCourses = null;
                    dto.RelatedOrgs = (await _orgUnitOfWork.QueryAsync<Guid, long?, MiniEvltRelatedOrgDto, MiniEvltRelatedOrgDto>(sql,
                        param: new { dto.Id },
                        splitOn: "Evaluationid,no,id",
                        map: (_, no, item) =>
                        {
                            if (no == null) return null;
                            item.Id_s = UrlShortIdUtil.Long2Base32(no.Value);
                            return item;
                        }
                    )).Where(_ => _ != null).AsArray();
                }
                else
                {
                    dto.RelatedMode = (int)EvltRelatedModeEnum.Other;
                    dto.RelatedCourses = null;
                    dto.RelatedOrgs = null;
                }

                await redis.SetAsync(CacheKeys.MiniEvltRelateds.FormatWith(dto.Id, agentType),
                    (new { dto.RelatedMode, dto.RelatedCourses, dto.RelatedOrgs }).ToJsonString(camelCase: true),
                    60 * 30);
            }

            // get 商品数量s
            if (dto.RelatedMode == (int)EvltRelatedModeEnum.Org && dto.RelatedOrgs?.Any() == true)
            {
                var dict = (await _mediator.Send(new MiniGetOrgsGoodsCountsQuery { OrgIds = dto.RelatedOrgs.Select(_ => _.Id).ToArray() })).Dict;
                if (dict != null)
                {
                    foreach (var rorg in dto.RelatedOrgs)
                        rorg.GoodsCount = dict.GetValueEx(rorg.Id, 0);
                }
            }
        }

    }
}
