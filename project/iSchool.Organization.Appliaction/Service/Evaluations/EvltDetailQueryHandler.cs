using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class EvltDetailQueryHandler : IRequestHandler<EvltDetailQuery, EvltDetailDto>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IMapper mapper;
        IConfiguration config;
        BussTknOption bussTknOption;

        const int cache_exp = 60 * 30;

        public EvltDetailQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis, IMapper mapper, IConfiguration config,
            IOptionsSnapshot<BussTknOption> bussTknOption)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.mapper = mapper;
            this.config = config;
            this.bussTknOption = bussTknOption.Value;
        }

        public async Task<EvltDetailDto> Handle(EvltDetailQuery req, CancellationToken cancellation)
        {
            var lsT = new List<Task>();
            var dto = await GetInfo0(new EvltDetailDto(), req);
            dto.Now = DateTime.Now;
            if (me.IsAuthenticated && me.UserId == dto.AuthorId)
            {
                dto.AuthorName = me.UserName;
                dto.AuthorHeadImg = me.HeadImg;
                dto.IsSelf = true;
            }
            else
            {
                lsT.Add(GetAuthor(dto));
            }
            if (req.AllowRecordPV)
            {
                // 增加PV
                AsyncUtils.StartNew(new PVisitEvent { CttId = dto.Id, UserId = me.UserId, Now = dto.Now, CttType = PVisitCttTypeEnum.Evaluation });
            }
            await GetContents(dto, req.No);
            await GetCoursePart(dto, req.No);
            await GetVote(dto, req.No);
            await GetComments(dto);
            await CheckIsCollect(dto);
            await CheckEditable(dto);
            await Task.WhenAll(lsT).ConfigureAwait(false);
            return dto;
        }

        async Task GetAuthor(EvltDetailDto dto)
        {
            var au = await mediator.Send(new UserSimpleInfoQuery
            {
                UserIds = new[] { dto.AuthorId }
            });
            dto.AuthorName = au.FirstOrDefault()?.Nickname;
            dto.AuthorHeadImg = au.FirstOrDefault()?.HeadImgUrl;
        }

        // 基本信息
        async Task<EvltDetailDto> GetInfo0(EvltDetailDto dto, EvltDetailQuery req)
        {
            var evltId = req.EvltId;
            string sql = null;            
            dynamic dy = (object)null;            

            var baseInfo = await mediator.Send(new GetEvltBaseInfoQuery { No = req.No, EvltId = req.EvltId });
            mapper.Map(baseInfo, dto);
            evltId = dto.Id;
            if (req.No == default) req.No = baseInfo.No;

            // query 统计数
            //
            dto.FirstCommentCount=dto.CollectionCount = dto.CommentCount = -1;
            var viewCount = -1;
            var rdkStatis = CacheKeys.EvaluationLikesCount.FormatWith(dto.Id);
            var dict = await redis.HGetAllAsync(rdkStatis);
            if (dict?.Any() == true)
            {
                dto.CollectionCount = int.Parse(dict.GetValueEx("collect", "-1"));
                viewCount = int.Parse(dict.GetValueEx("viewer", "-1"));                
                dto.CommentCount = int.Parse(dict.GetValueEx("comments", "-1"));
                dto.FirstCommentCount = int.Parse(dict.GetValueEx("firstcomments", "-1"));
            }
            if (dto.CollectionCount == -1 || dto.CommentCount == -1 || viewCount == -1)
            {
                sql = $@"
select evlt.Id,evlt.CollectionCount,evlt.CommentCount,evlt.ViewCount
from Evaluation evlt
where evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()}
and evlt.id=@evltId
";
                dy = await unitOfWork.QueryFirstOrDefaultAsync(sql, new { evltId = dto.Id, me.UserId });

                var pipe = redis.StartPipe();
                if (dto.CollectionCount == -1) pipe.HSet(rdkStatis, "collect", (object)dy?.CollectionCount ?? 0);                
                if (dto.CommentCount == -1) pipe.HSet(rdkStatis, "comments", (object)dy?.CommentCount ?? 0);
               
             
                if (viewCount == -1) pipe.HSet(rdkStatis, "viewer", (object)dy?.ViewCount ?? 0);
                pipe.Ttl(rdkStatis);
                var o = await pipe.EndPipeAsync();
                if (Equals(-1L, o.Last()))
                {
                    _ = redis.ExpireAsync(rdkStatis, 60 * 60 * 24 * 1);
                }

                dto.CollectionCount = dy == null ? 0 : Convert.ToInt32(dy.CollectionCount);                
                dto.CommentCount = dy == null ? 0 : Convert.ToInt32(dy.CommentCount);
                viewCount = dy == null ? 0 : Convert.ToInt32(dy.ViewCount);
            }
            if (dto.FirstCommentCount == -1)
            {
                sql = $@"
select count(1) from  EvaluationComment where evaluationid=@evltId and  fromid is null and IsValid=1 ";
                var first_commment_count = await unitOfWork.QueryFirstOrDefaultAsync<int>(sql, new { evltId = dto.Id });
                redis.HSet(rdkStatis, "firstcomments", first_commment_count);
                dto.FirstCommentCount = first_commment_count;

            }
            // likecount + IsLikeByMe
            var likes = await mediator.Send(new EvltLikesQuery { EvltIds = new[] { dto.Id } });
            if (likes.Items.TryGetValue(dto.Id, out var lk))
            {
                dto.LikeCount = lk.Likecount;
                dto.IsLikeByMe = lk.IsLikeByMe;
            }

            return dto;
        }

        // 内容s
        async Task GetContents(EvltDetailDto dto, long no)
        {
            var rdk = CacheKeys.Evlt.FormatWith(dto.Id);
            dto.Contents = await redis.HGetAsync<EvaluationContentDto[]>(rdk, "contents");

            if (dto.Contents == null)
            {
                var sql = $@"
select item.* from Evaluation evlt 
join EvaluationItem item on item.evaluationid=evlt.id
where evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()} and item.IsValid=1 and evlt.id=@Id and item.type{(dto.Mode == 1 ? "=" : ">")}0
order by item.type
";
                var items = await unitOfWork.QueryAsync<EvaluationItem>(sql, new { dto.Id });
                dto.Contents = items.Select(x => mapper.Map<EvaluationContentDto>(x)).ToArray();

                _ = redis.HSetAsync(rdk, "contents", dto.Contents);
            }

            var ctts = string.Join('\n', dto.Contents.Select(_ => _?.Content ?? ""));
            ctts = HtmlHelper.NoHTML(ctts);
            dto.SharedContent = ctts.Length > 50 ? ctts[0..50] : ctts;
            dto.Tdk_d = ctts.Length > 160 ? ctts[0..160] : ctts;
        }

        // 课程info
        async Task GetCoursePart(EvltDetailDto dto, long no)
        {
            var rdk = CacheKeys.Evlt.FormatWith(dto.Id);
            dto.CoursePart = await redis.HGetAsync<EvaluationCoursePartDto>(rdk, "course");
            if (dto.CoursePart != null) return;
            dto.CoursePart = new EvaluationCoursePartDto();

            // c.name c.title
            // eb.coursename暂无
            var sql = $@"
select evlt.id,eb.orgid,org.no as orgid_s,(case when eb.orgid is null then eb.orgname else org.name end)as OrgName,org.logo as orglogo,org.authentication as OrgIsAuthenticated,
(case when eb.orgid is null then null else org.[desc] end)as OrgDesc,
(case when eb.orgid is null then null else org.subdesc end)as OrgSubdesc,
c.id as courseid,c.no as CourseId_s,(case when eb.courseid is null then eb.coursename else c.title end)as CourseName,c.banner as CourseBanner,
(case when eb.courseid is null then null else c.subtitle end)as CourseSubtitle,
(case when eb.courseid is null then eb.price else c.price end)as price,
(case when eb.courseid is null then eb.mode else c.mode end)as mode,
(case when eb.courseid is null then eb.opentime else c.opentime end)as opentime,
(case when eb.courseid is null then eb.duration else c.duration end)as duration,
(case when eb.courseid is null then eb.cycle else c.cycle end)as cycle,
(case when eb.courseid is null then eb.age else null end)as age, --c.age
(case when eb.courseid is null then null else c.minage end)as minage,
(case when eb.courseid is null then null else c.maxage end)as maxage,
(case when eb.courseid is null then eb.subject else c.subject end)as subject
from Evaluation evlt 
left join EvaluationBind eb on eb.evaluationid=evlt.id and eb.isvalid=1
left join Organization org on org.id=eb.orgid and org.isvalid=1 and org.status={OrganizationStatusEnum.Ok.ToInt()}
left join Course c on c.id=eb.courseid and c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and c.type={CourseTypeEnum.Course.ToInt()}
where evlt.IsValid=1 and evlt.status={EvaluationStatusEnum.Ok.ToInt()} and evlt.id=@Id
";
            var dy = await unitOfWork.QueryFirstOrDefaultAsync(sql, new { dto.Id });
            if (dy != null) parseByCoursePart(dto.CoursePart, dy);

            _ = redis.HSetAsync(rdk, "course", dto.CoursePart);
        }

        // 投票
        async Task GetVote(EvltDetailDto dto, long no)
        {
            var rdk = CacheKeys.Evlt.FormatWith(dto.Id);
            var rdk1 = "vote";
            var str_vote = await redis.HGetAsync(rdk, rdk1);
            if (str_vote != null)
            {
                if (str_vote == "{}" || str_vote == "") dto.Vote = null;
                else dto.Vote = str_vote.ToObject<EvaluationVoteDto>();                
            }
            else
            {
                var sql = @"
select v.evaluationid,v.Id as voteId,v.title,v.detail,v.type,v.endtime,
i.id as voteItemId,i.content,i.sort,i.count
from EvaluationVote v
left join Evaluation evlt on evlt.id=v.evaluationid and evlt.IsValid=1
left join EvaluationVoteItems i on i.voteId=v.id and i.IsValid=1
where v.IsValid=1 and evlt.Id=@Id
order by i.sort
";
                var dys = await unitOfWork.QueryAsync(sql, new { dto.Id, me.UserId });
                parseByVote(dto.Id, out var vote, dys);
                dto.Vote = vote;
                if (dto.Vote != null) await redis.HSetAsync(rdk, rdk1, dto.Vote);
                else await redis.HSetAsync(rdk, rdk1, "{}");
            }

            // return if no vote            
            if (dto.Vote == null)
            {
                return;
            }

            // find voteitem's counts
            //
            var rdk_v = CacheKeys.EvltVote.FormatWith(dto.Vote.Id);
            var dict_v = await redis.HGetAllAsync(rdk_v);
            if (dict_v?.Any() == true)
            {
                foreach (var (voteItemId, c) in dict_v)
                {
                    var gid_voteItemId = Guid.Parse(voteItemId);
                    var item = dto.Vote.Items.FirstOrDefault(_ => _.Id == gid_voteItemId);
                    if (item == null) continue;
                    item.Count = int.TryParse(c, out var _c) ? _c : default;
                }
            }
            else
            {
                var sql = @"
select id,count from EvaluationVoteItems where IsValid=1 and id in @Ids
";
                var dys = await unitOfWork.QueryAsync(sql, new { Ids = dto.Vote.Items.Select(_ => _.Id) });
                var ls = new List<object>();
                foreach (var dy in dys)
                {
                    Guid gid = Guid.Parse(dy.id.ToString());
                    var item = dto.Vote.Items.FirstOrDefault(_ => _.Id == gid);
                    if (item == null) continue;
                    item.Count = int.TryParse((string)(dy.count.ToString()), out var _c) ? _c : default;
                    ls.Add(gid);
                    ls.Add(item.Count);
                }
                await redis.StartPipe()
                    .HMSet(rdk_v, ls.ToArray())
                    .Expire(rdk_v, 60 * 60 * 6)
                    .EndPipeAsync();
            }

            // find IsVotedByMe            
            if (me.IsAuthenticated)
            {
                var rdk_me = CacheKeys.MyEvltVote.FormatWith(("userid", me.UserId), ("evltId", dto.Id));
                var vdict = await redis.HGetAllAsync(rdk_me);
                if (vdict?.Any() != true)
                {
                    var sql = @"
select vi.id as Item1,(case when s.id is not null then 1 else 0 end)as Item2
from EvaluationVoteItems vi 
left join EvaluationVoteSelect s on s.IsValid=1 and s.voteItemId=vi.id and s.userid=@UserId 
where vi.IsValid=1 and vi.voteid=@VoteId
";
                    var dys = await unitOfWork.QueryAsync(sql, new { VoteId = dto.Vote.Id, me.UserId });
                    vdict = dys.ToDictionary(_ => $"{dto.Vote.Id}_{_.Item1}", _ => $"{_.Item2}");

                    var pipe = redis.StartPipe();
                    foreach (var kv in vdict)
                        pipe.HSet(rdk_me, kv.Key, kv.Value);
                    if (!vdict.Any()) pipe.HSet(rdk_me, "null", 0);
                    pipe.Expire(rdk_me, 60 * 60 * 6);
                    await pipe.EndPipeAsync();
                }
                foreach (var (viid, isv) in vdict)
                {
                    if (isv != "1") continue;
                    var iid = Guid.Parse(viid[37..]);
                    var item = dto.Vote.Items.FirstOrDefault(_ => _.Id == iid);
                    if (item == null) continue;
                    item.IsVoteByMe = true;
                }
            }
            dto.Vote.IsVotedByMe = dto.Vote.Items.Any(itm => itm.IsVoteByMe);

            // check 投票过期
            if (dto.Vote.EndTime != null && dto.Vote.EndTime.Value <= dto.Now) dto.Vote.CanShowCount = true;
            else dto.Vote.CanShowCount = dto.Vote.IsVotedByMe;
        }

        // 评论前n条
        async Task GetComments(EvltDetailDto dto)
        {
            var pg = await mediator.Send(new EvltCommentsQuery 
            {
                EvltId = dto.Id,
                PageIndex = 1,
                PageSize = 20,
                Naf = dto.Now,
            });            
            dto.Comments = pg.CurrentPageItems.AsArray();
            foreach (var m in dto.Comments)
            {
                m.Now = dto.Now;
            }
        }

        async Task CheckEditable(EvltDetailDto dto)
        {
            if (dto.SpecialId == null) return;
            var editable = await mediator.Send(new CheckEvltEditableQuery { EvltId = dto.Id });
            dto.Editable = editable.Enable;
            if (!dto.Editable)
            { 
                var path = Path.Combine(Directory.GetCurrentDirectory(), config[$"AppSettings:hd2:hd_tile_qrcode"]);
                var bys = await File.ReadAllBytesAsync(path);
                dto.EditdisableQrcode = $"data:image/png;base64,{Convert.ToBase64String(bys)}";
            }
        }

        void parseByCoursePart(EvaluationCoursePartDto coursePart, dynamic dy)
        {
            if (dy.orgid != null) coursePart.OrgId = Guid.Parse(dy.orgid.ToString());
            if (dy.orgid_s != null) coursePart.OrgId_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(dy.orgid_s));
            if (dy.OrgName != null) coursePart.OrgName = dy.OrgName;
            if (dy.OrgDesc != null) coursePart.OrgDesc = dy.OrgDesc;
            if (dy.OrgSubdesc != null) coursePart.OrgSubdesc = dy.OrgSubdesc;
            if (dy.orglogo != null) coursePart.OrgLogo = dy.orglogo;
            if (dy.OrgIsAuthenticated != null) coursePart.OrgIsAuthenticated = Convert.ToBoolean(dy.OrgIsAuthenticated);
            else if (dy.orgid_s == null) coursePart.OrgIsAuthenticated = false;
            if (dy.orgid_s != null && dy.courseid != null) coursePart.CourseId = Guid.Parse(dy.courseid.ToString());
            if (dy.orgid_s != null && dy.CourseId_s != null) coursePart.CourseId_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(dy.CourseId_s));
            if (dy.orgid_s != null && dy.CourseName != null) coursePart.CourseName = dy.CourseName;
            if (dy.CourseSubtitle != null) coursePart.CourseSubtitle = dy.CourseSubtitle;
            if (dy.CourseBanner != null) coursePart.CourseBanner = ((string)dy.CourseBanner?.ToString())?.ToObject<string[]>();
            if (dy.price != null) coursePart.Price = Convert.ToDecimal(dy.price);                        
            if (dy.mode != null)
            {
                coursePart.Mode = ((string)dy.mode.ToString()).ToObject<TeachModeEnum[]>().Select(_ => EnumUtil.GetDesc(_)).ToArray();
                if (coursePart.Mode.Length < 1) coursePart.Mode = null;
            }
            if (dy.opentime != null) coursePart.OpenTime = DateTime.Parse(dy.opentime.ToString());
            if (dy.duration != null)
            {
                var v = EnumUtil.GetDesc(((string)dy.duration.ToString()).ToEnum<CourceDurationEnum>());
                if (string.IsNullOrEmpty(v)) v = $"{dy.duration}分钟";
                coursePart.Duration = v;
            }
            if (dy.cycle != null) coursePart.Cycle = dy.cycle.ToString();

            // 科目
            if (dy.orgid_s != null && dy.subject != null)
            {
                var em = ((string)dy.subject.ToString()).ToEnum<SubjectEnum>();                
                coursePart.Subj = (int)em;
                coursePart.Subject = EnumUtil.GetDesc(em);
            }
            else
            {
                coursePart.Subj = SubjectEnum.Other.ToInt();
            }

            // 年龄段
            if ((dy.minage == null && dy.maxage == null) || (dy.minage == 0 && dy.maxage == 0))
            {
                if (dy.age != null) coursePart.Age = EnumUtil.GetDesc(((string)dy.age.ToString()).ToEnum<AgeGroup>());
            }
            else
            {
                coursePart.Age = $"{(dy.minage ?? 0)}-{(dy.maxage ?? 0)}";
            }
        }

        void parseByVote(Guid evltId, out EvaluationVoteDto dto, IEnumerable<dynamic> dys)
        {
            dto = null;
            var i = 0;
            foreach (IDictionary<string, object> dy in dys)
            {
                if (dto == null)
                {
                    dto = new EvaluationVoteDto();
                    dto.Id = Guid.Parse(dy["voteId"].ToString());
                    dto.Title = dy["title"].ToString();
                    dto.Detail = dy.GetValueEx("detail")?.ToString();
                    dto.Type = Convert.ToByte(dy["type"]);
                    dto.EndTime = dy["endtime"]?.ToString() is string end ? DateTime.Parse(end) : (DateTime?)null;
                    dto.Items = new EvaluationVoteItemDto[dys.Count()];
                }
                var itmdto = dto.Items[i] ??= new EvaluationVoteItemDto();
                itmdto.Id = Guid.Parse(dy["voteItemId"].ToString());
                itmdto.Content = dy.GetValueEx("content")?.ToString();
                itmdto.Token = TokenHelper.CreateStokenByJwt(bussTknOption.Key, bussTknOption.Alg, bussTknOption.Exp, itmdto.Id, dto.Id, evltId);
                itmdto.Count = Convert.ToInt32(dy["count"]);
                //itmdto.IsVoteByMe = Convert.ToBoolean(dy["IsVotedByMe"]);

                i++;
            }
        }

        async Task CheckIsCollect(EvltDetailDto dto)
        {                               
            if (me.IsAuthenticated)
            {
                var sql = $@"select count(1) from [Collection] where datatype={CollectionEnum.Evaluation.ToInt()} and userId='{me.UserId}'
and dataId='{dto.Id}' and isvalid=1;
";
                var count = await unitOfWork.QueryFirstAsync<int>(sql, null);
                dto.IsCollectByMe = count > 0;
            }
           
        }
    }
}
