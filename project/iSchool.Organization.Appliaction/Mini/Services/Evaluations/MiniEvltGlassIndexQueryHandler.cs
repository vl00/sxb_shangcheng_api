using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Common;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class MiniEvltGlassIndexQueryHandler : IRequestHandler<MiniEvltGlassIndexQuery, MiniEvltGrassIndexQryResult>
    {
        private readonly OrgUnitOfWork _orgUnitOfWork;
        private readonly IMediator _mediator;
        private readonly IUserInfo me;
        private readonly CSRedisClient redis;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        const int cache_exp = 60 * 30;

        public MiniEvltGlassIndexQueryHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator, IUserInfo me, IHttpContextAccessor httpContextAccessor,
            CSRedisClient redis, IMapper mapper, IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;
            this._mapper = mapper;
            this._config = config;
            this._httpContextAccessor = httpContextAccessor;
        }

        public async Task<MiniEvltGrassIndexQryResult> Handle(MiniEvltGlassIndexQuery query, CancellationToken cancellation)
        {
            var result = new MiniEvltGrassIndexQryResult();
            if (me.IsAuthenticated) result.Me = me;
            if (query.Brand == Guid.Empty) query.Brand = null;
            if (query.CourseId == Guid.Empty) query.CourseId = null;

            result.Orderbys = _config.GetSection("AppSettings:mini_evltgrassIndex:orderbys").GetChildren().ToDictionary(_ => _["key"], _ => _["value"]).ToArray();
            result.Ctts = _config.GetSection("AppSettings:mini_evltgrassIndex:ctts").GetChildren().ToDictionary(_ => _["key"], _ => _["value"]).ToArray();
            result.Subjs = _config.GetSection("AppSettings:mini_evltgrassIndex:subjs").GetChildren().ToDictionary(_ => _["key"], _ => _["value"]).ToArray();
            //添加好物的分类
            var goodThingDict = new Dictionary<string, string>();
            var listGoodThingType = (List<SelectItemsKeyValues>)_mediator.Send(new KeyValueSelectItemsQuery() { Type = 14 }).Result.Data;
            foreach (var item in listGoodThingType)
            {
                goodThingDict.Add(item.Value, item.Key.ToString());
            }
            result.Subjs = result.Subjs.Union(goodThingDict);

            result.Brands = ((await _mediator.Send(new KeyValueSelectItemsQuery { Type = 0 })).Data as IEnumerable<SelectItemsKeyValues>)?.ToDictionary(_ => _.Key.ToString(), _ => _.Value).ToArray();

            // page
            var key = CacheKeys.MiniEvltGrassIndex.FormatWith(query.PageSize, query.Orderby, query.Brand, query.CatogoryId, query.Ctt, query.CourseId);
            if (query.PageIndex == 1)
            {
                var jstr = await redis.GetAsync(key);
                if (!string.IsNullOrEmpty(jstr))
                {
                    var jo = JObject.Parse(jstr);
                    result.PageInfo = jo["page1_items"].ToObject<MiniEvaluationItemDto[]>().ToPagedList(query.PageSize, query.PageIndex, (int)jo["totalItemCount"]);
                }
            }
            if (result.PageInfo == null)
            {
                var sqlwhere = new StringBuilder();
                var sql = $@"
select count(1) from Evaluation evlt
where evlt.IsValid=1 and evlt.status=@status {{0}}
---
select evlt.*,(select Id,Evaluationid,Type,Content,JSON_QUERY(Pictures)as Pictures,JSON_QUERY(Thumbnails)as Thumbnails,Video,VideoCover
    from EvaluationItem item where item.evaluationid=evlt.id and item.IsValid=1 order by item.type for json path)as contents 
from (
select evlt.*,(evlt.likes+evlt.shamlikes)as _likes from Evaluation evlt
where evlt.IsValid=1 and evlt.status=@status {{0}} --and evlt.id='CDC9FDF5-5A30-43B8-8F4F-1AA634A06B00'
) evlt
order by {(query.Orderby switch
                {
                    1 => "_likes desc,evlt.CreateTime desc",
                    2 => "evlt.SharedTime desc,evlt.CreateTime desc,_likes desc",
                    _ => "evlt.CreateTime desc,_likes desc",
                })} 
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
                sqlwhere.AppendLine($@"{"and evlt.isPlaintext=0".If(query.Ctt == 1)} {"and evlt.HasVideo=1".If(query.Ctt == 2)}");
                if (query.Brand != null && query.Brand != Guid.Empty)
                {
                    sqlwhere.AppendLine($@"
and exists(select 1 from EvaluationBind b where b.IsValid=1 and b.evaluationid=evlt.id and b.orgid=@Brand) ");
                }
                if (query.CourseId != null && query.CourseId != Guid.Empty)
                {
                    sqlwhere.AppendLine($@"
and exists(select 1 from EvaluationBind b where b.IsValid=1 and b.evaluationid=evlt.id and b.CourseId=@CourseId) ");
                }
                var dyp = new DynamicParameters(query);
                if (!string.IsNullOrEmpty(query.CatogoryId))
                {


                    sqlwhere.AppendLine($@"
and (
 exists(select 1 from EvaluationBind b join Course c on c.id=b.courseid and c.IsValid=1 and c.status={CourseStatusEnum.Ok.ToInt()} and c.IsInvisibleOnline=0
	where b.IsValid=1 and b.evaluationid=evlt.id and b.courseid is not null
        and (select count(1) from openjson(c.CommodityTypes) j1 where  j1.[value]=@CatogoryId)>0
)) ");
                    dyp.Set("@CatogoryId", query.CatogoryId);

                }

                sql = sql.FormatWith(sqlwhere);

                dyp.Set("@status", EvaluationStatusEnum.Ok.ToInt());

                var gr = await _orgUnitOfWork.QueryMultipleAsync(sql, dyp);
                var cc = await gr.ReadFirstAsync<int>();
                var items = (gr.Read<Evaluation, string, MiniEvaluationItemDto>(splitOn: "contents", func: (evlt, contents) =>
                {
                    var item = new MiniEvaluationItemDto();
                    item.Id = evlt.Id;
                    item.Id_s = UrlShortIdUtil.Long2Base32(evlt.No);
                    item.Title = evlt.Title;
                    item.Stick = evlt.Stick;
                    item.CreateTime = evlt.CreateTime;
                    item.AuthorId = evlt.Userid;
                    item.Content = contents;
                    item.Cover = evlt.Cover;
                    return item;
                })).AsArray();

                // resolve contents
                foreach (var item in items)
                {
                    var contents = item.Content;
                    if (string.IsNullOrEmpty(contents)) continue;
                    var jarr = JArray.Parse(contents);
                    if (jarr == null) continue;
                    item.Content = string.Join('\n', jarr.Select(j => j["Content"]?.ToString() ?? ""));
                    item.Imgs = jarr.SelectMany(j => j["Pictures"].ToObject<string[]>() ?? new string[0]).ToArray();
                    item.Imgs_s = jarr.SelectMany(j => j["Thumbnails"].ToObject<string[]>() ?? new string[0]).ToArray();
                    item.VideoUrl = jarr.FirstOrDefault(j => (int?)j["Type"] == 0)?["Video"]?.ToString();
                    item.VideoCoverUrl = jarr.FirstOrDefault(j => (int?)j["Type"] == 0)?["VideoCover"]?.ToString();
                }

                result.PageInfo = items.ToPagedList(query.PageSize, query.PageIndex, cc);

                if (query.PageIndex == 1)
                {
                    await redis.SetAsync(key, new { totalItemCount = cc, page1_items = items }, 60 * 15);
                }
            }

            // 查用户信息
            if (result.PageInfo.CurrentPageItems.Any())
            {
                var uInfos = await _mediator.Send(new UserSimpleInfoQuery
                {
                    UserIds = result.PageInfo.CurrentPageItems.Select(_ => _.AuthorId)
                });
                foreach (var item in result.PageInfo.CurrentPageItems)
                {
                    if (!uInfos.TryGetOne(out var u, _ => _.Id == item.AuthorId)) continue;
                    item.AuthorName = u.Nickname;
                    item.AuthorHeadImg = u.HeadImgUrl;
                }
            }

            // find likecount + IsLikeByMe
            if (result.PageInfo.CurrentPageItems.Any())
            {
                var likes = await _mediator.Send(new EvltLikesQuery { EvltIds = result.PageInfo.CurrentPageItems.Select(_ => _.Id).ToArray() });
                foreach (var item in result.PageInfo.CurrentPageItems)
                {
                    if (!likes.Items.TryGetValue(item.Id, out var lk)) continue;
                    item.LikeCount = lk.Likecount;
                    item.IsLikeByMe = lk.IsLikeByMe;
                }
            }

            // 分享数
            if (result.PageInfo.CurrentPageItems.Any())
            {
                var sitems = await _mediator.Send(new GetEvltMiniSharedCountsQueryArgs(result.PageInfo.CurrentPageItems.Select(_ => _.Id).ToArray()));
                foreach (var item in result.PageInfo.CurrentPageItems)
                {
                    if (!sitems.TryGetOne(out var s, (_) => _.EvltId == item.Id)) continue;
                    item.SharedCount = s.SharedCount;
                }
            }

            // 关联主体
            if (result.PageInfo.CurrentPageItems.Any())
            {
                var rr = await _mediator.Send(new GetEvltRelatedsQueryArgs { EvltIds = result.PageInfo.CurrentPageItems.Select(_ => _.Id).ToArray() });
                foreach (var item in result.PageInfo.CurrentPageItems)
                {
                    item.RelatedMode = (int)EvltRelatedModeEnum.Other;
                    if (!rr.TryGetOne(out var x, _ => _.EvltId == item.Id)) continue;
                    item.RelatedMode = x.RelatedMode;
                    item.RelatedCourses = x.RelatedCourses;
                    item.RelatedOrgs = x.RelatedOrgs;
                }
            }

            return result;
        }

    }
}
