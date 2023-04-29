using AutoMapper;
using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Activity.Appliaction.RequestModels;
using iSchool.Organization.Activity.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Activity.Appliaction.Service
{
    public class EvaluationLikePageQueryHandler : IRequestHandler<EvaluationLikePageQuery, EvaluationLikePageResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IConfiguration config;
        IMapper mapper;

        public EvaluationLikePageQueryHandler(IOrgUnitOfWork unitOfWork, IMapper mapper,
            IMediator mediator, CSRedisClient redis,
            IConfiguration config,
            IUserInfo me)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.config = config;
            this.mapper = mapper;
        }

        public async Task<EvaluationLikePageResult> Handle(EvaluationLikePageQuery query, CancellationToken cancellation)
        {
            var result = new EvaluationLikePageResult();
            string sql = null;
            await Task.CompletedTask;

            var ainfo = await mediator.Send(new ActivitySimpleInfoQuery { Id = query.ActivityInfo.ActivityId });
            if (ainfo == null) throw new CustomResponseException("活动无效");
            result.ActivityData = mapper.Map<ActivityDataDto>(ainfo);
            if (result.ActivityData.Astatus != 0)
            {
                goto lb_end;
            }

            result.Pcode = query.ActivityInfo.IsHasPromo ? query.ActivityInfo.Promocode : query.ActivityInfo.Acode;
            result.Banner = config["AppSettings:hd1:banner_main"];

            // list/page数据
            //
            if (query.PageIndex != 1) throw new CustomResponseException("暂只支持查询第一页");
            var items = await redis.GetAsync<EvaluationItemDto[]>(CacheKeys.Hd1_main);
            if (items == null)
            {
                sql = $@"
select top {query.PageSize} a.id as activityid,e.id,e.no as id_s,e.title,e.stick,e.userid as AuthorId,e.status,e.IsPlaintext,e.cover,e.CreateTime,e.likes
from Activity a
left join Special s on s.activity=a.id and s.IsValid=1 and s.status={SpecialStatusEnum.Ok.ToInt()}
left join SpecialBind sb on sb.specialid=s.id and sb.IsValid=1
left join Evaluation e on e.id=sb.evaluationid and e.IsValid=1
where a.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()} and a.id=@ActivityId
order by e.Likes desc,e.CreateTime asc
-- OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
                items = unitOfWork.DbConnection.Query<EvaluationItemDto>(sql, new { query.PageIndex, query.PageSize, query.ActivityInfo.ActivityId }).AsArray();
                foreach (var item in items)
                {
                    item.Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(item.Id_s));
                }
                await redis.SetAsync(CacheKeys.Hd1_main, items, 60 * 2);
            }
            result.PageInfo = new PagedList<EvaluationItemDto>();
            result.PageInfo.CurrentPageIndex = query.PageIndex;
            result.PageInfo.PageSize = query.PageSize;
            result.PageInfo.TotalItemCount = items.Length;
            result.PageInfo.CurrentPageItems = items;

            var rr = await mediator.Send(new UserSimpleInfoQuery { UserIds = items.Select(_ => _.AuthorId).Distinct() });
            foreach (var item in items)
            {
                var r = rr.FirstOrDefault(_ => _.Id == item.AuthorId);
                if (r == null) continue;
                item.AuthorName = r.Nickname;
                item.AuthorHeadImg = r.HeadImgUrl ?? config["AppSettings:UserDefaultHeadImg"];
            }

            var rr1 = await mediator.Send(new EvltLikesQuery { EvltIds = items.Select(_ => _.Id).Distinct().ToArray() });
            foreach (var item in items)
            {
                if (!rr1.Items.TryGetValue(item.Id, out var r)) continue;
                item.LikeCount = r.Likecount;
                item.IsLikeByMe = r.IsLikeByMe;
            }

            // 我刚刚发布的评测item
            var hasLastestEvltAddedByMe = false;
            if (me.IsAuthenticated)
            {
                var item = await redis.GetAsync<EvaluationItemDto>(CacheKeys.Hd1_UserLastestEvltAdded.FormatWith(me.UserId));
                if (item != null && item.AuthorId == me.UserId)
                {
                    _ = redis.DelAsync(CacheKeys.Hd1_UserLastestEvltAdded.FormatWith(me.UserId));
                    hasLastestEvltAddedByMe = true;
                    result.EvltAddedByMe = item;
                }
            }

            // 用户评测点赞排名最高的信息
            if (me.IsAuthenticated)
            {
                var uelh = await redis.GetAsync<UseEvltLikeHighestInfo>(CacheKeys.Hd1_UserEvltLikeRankData.FormatWith(me.UserId));
                if (uelh == null || (hasLastestEvltAddedByMe && uelh.Id == default))
                {
                    sql = $@"
select top 1 a.activityid,a.likecount,a.top_no as LikeRank,e.id,e.no as id_s,e.CreateTime,a.userid,e.commentcount,e.cover
from ActivityUserEvltLikeRank a with(nolock) 
left join Evaluation e on a.evaluationid=e.id 
where a.IsValid=1 and e.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()} 
and a.activityid=@ActivityId and a.userid=@UserId
order by a.top_no asc
---
select max(top_no) from ActivityUserEvltLikeRank with(nolock) where IsValid=1 
";
                    var gg = await unitOfWork.DbConnection.QueryMultipleAsync(sql, new { me.UserId, query.ActivityInfo.ActivityId });
                    uelh = gg.ReadFirstOrDefault<UseEvltLikeHighestInfo>();                    
                    if (uelh != null)
                    {
                        uelh.Id_s = UrlShortIdUtil.Long2Base32(Convert.ToInt64(uelh.Id_s));
                    }
                    else if (hasLastestEvltAddedByMe)
                    {
                        var lastNo = gg.ReadFirstOrDefault<int?>() ?? 100; 
                        uelh = new UseEvltLikeHighestInfo();
                        uelh.LikeRank = lastNo + 1;
                        uelh.Id = result.EvltAddedByMe!.Id;
                        uelh.Id_s = result.EvltAddedByMe!.Id_s;
                        uelh.UserId = me.UserId;
                        uelh.Cover = result.EvltAddedByMe!.Cover;
                    }

                    await redis.SetAsync(
                        CacheKeys.Hd1_UserEvltLikeRankData.FormatWith(me.UserId),
                        uelh == null ? "{}" : uelh.ToJsonString(true),
                        60 * 1
                    );
                }
                result.UseEvltLikeHighestInfo = uelh?.UserId != me.UserId ? null : uelh;
            }            

            lb_end:
            return result;
        }

        
    }
}
