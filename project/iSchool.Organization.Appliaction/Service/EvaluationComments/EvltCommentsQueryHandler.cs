using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class EvltCommentsQueryHandler : IRequestHandler<EvltCommentsQuery, PagedList<EvaluationCommentDto>>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IConfiguration config;

        public EvltCommentsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, CSRedisClient redis,
            IConfiguration config)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.config = config;
        }

        public async Task<PagedList<EvaluationCommentDto>> Handle(EvltCommentsQuery req, CancellationToken cancellation)
        {
            var now = DateTime.Now;
            var pg = new PagedList<EvaluationCommentDto>();
            pg.PageSize = req.PageSize;
            pg.CurrentPageIndex = req.PageIndex;
            Task tsk_findUser = null;
            string sql = null;
            await default(ValueTask);
            EvaluationCommentDto[] items0 = null;
            var totalItemCount = 0;
            if (pg.CurrentPageIndex == 1)
            {
                var rdk = CacheKeys.EvltCommentTopN.FormatWith(req.PageSize, req.EvltId);
                var jtk= await redis.GetAsync<JToken>(rdk);
                if (null!=jtk)
                {
                   
                    try
                    {
                        totalItemCount = (int)jtk["totalItemCount"];
                        items0 = jtk["page1_items"].ToObject<EvaluationCommentDto[]>();
                    }
                    catch (Exception)
                    {

                        items0 = null;
                    }
                }
                if (items0 != null)
                {
                    pg.TotalItemCount = totalItemCount;
                    pg.CurrentPageItems = items0;
                    goto LB_user;
                }
            }
            sql = $@"
select count(1) from Evaluation evlt join EvaluationComment cmmt on evlt.id=cmmt.evaluationid 
where cmmt.fromid is null and evlt.IsValid=1 and cmmt.IsValid=1 and evlt.id=@EvltId {"and evlt.CreateTime<@Naf and cmmt.CreateTime<=@Naf".If(req.Naf != null)}
;;
select cmmt.id,cmmt.userid,cmmt.CreateTime,cmmt.comment,
cmmt.username,cmmt.likes,cmmt.CommentCount,evlt.userid as authorid
from Evaluation evlt
join EvaluationComment cmmt on evlt.id=cmmt.evaluationid
where cmmt.fromid is null and  evlt.IsValid=1 and cmmt.IsValid=1 and evlt.id=@EvltId {"and evlt.CreateTime<@Naf and cmmt.CreateTime<=@Naf".If(req.Naf != null)}
order by cmmt.likes desc,cmmt.CommentCount desc,CreateTime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";    
            var gr = await unitOfWork.QueryMultipleAsync(sql,
                new DynamicParameters(req).Set(nameof(me.UserId), me.UserId));
            pg.TotalItemCount = await gr.ReadFirstAsync<int>();
            pg.CurrentPageItems = await gr.ReadAsync<EvaluationCommentDto>();

            if (pg.CurrentPageIndex == 1)
            {
                var rdk = CacheKeys.EvltCommentTopN.FormatWith(req.PageSize, req.EvltId);
                await redis.SetAsync(rdk,new { totalItemCount = pg.TotalItemCount, page1_items = pg.CurrentPageItems }, 60 * 15);
            }

            LB_user:
            do
            {
                var lsU = new List<EvaluationCommentDto>();
                foreach (var item in pg.CurrentPageItems)
                {
                    item.Now = now;
                    item.IsAuthor = item.UserId == item.AuthorId && item.UserId != default;
                    if (me.IsAuthenticated && item.UserId == me.UserId)
                    {
                        item.IsMy = true;
                        item.Username = me.UserName;
                        item.UserImg = me.HeadImg;
                    }
                    else
                    {
                        item.IsMy = false;
                        lsU.Add(item);
                    }
                }
                if (!lsU.Any()) break;
                tsk_findUser = FindUserInfo(lsU);
            }
            while (false);

            // like count + IsLikeByMe
            {
                var lks = await mediator.Send(new EvltCommentLikesQuery { Ids = pg.CurrentPageItems.Select(_ => (req.EvltId, _.Id)).ToArray() });
                foreach (var item in pg.CurrentPageItems)
                {
                    if (!lks.Items.TryGetValue((req.EvltId, item.Id), out var v)) continue;
                    item.Likes = v.Likecount;
                    item.IsLikeByMe = v.IsLikeByMe;
                }
                #region old code
//                do
//                {
//                    var rdk = CacheKeys.EvaluationCommentLikesCount.FormatWith(req.EvltId);
//                    var cc = await redis.HMGetAsync(rdk, pg.CurrentPageItems.Select(_ => _.Id.ToString()).ToArray());
//                    var ls = new List<EvaluationCommentDto>();
//                    var i = 0;
//                    foreach (var item in pg.CurrentPageItems)
//                    {
//                        if (cc == null || cc.Length <= i || cc[i] == null) ls.Add(item);
//                        else item.Likes = Convert.ToInt32(cc[i]);
//                        i++;
//                    }
//                    if (!ls.Any()) break;

//                    sql = @"select Id as Item1,likes as Item2 from EvaluationComment where IsValid=1 and id in @Ids";
//                    var likes = await unitOfWork.DbConnection.QueryAsync<(Guid, int)>(sql, new { Ids = ls.Select(_ => _.Id) });
//                    foreach (var like in likes)
//                    {
//                        var item = ls.FirstOrDefault(_ => _.Id == like.Item1);
//                        if (item == null) continue;
//                        item.Likes = like.Item2;
//                    }

//                    var pipe = redis.StartPipe();
//                    foreach (var item in ls)
//                        pipe.HSet(rdk, item.Id.ToString(), item.Likes);
//                    pipe.Expire(rdk, 60 * 60 * 6);
//                    await pipe.EndPipeAsync();
//                }
//                while (false);

//                // IsLikeByMe
//                while (me.IsAuthenticated)
//                {
//                    var rdk = CacheKeys.MyCommentLikes.FormatWith(me.UserId, req.EvltId);
//                    var cc = await redis.HMGetAsync(rdk, pg.CurrentPageItems.Select(_ => _.Id.ToString()).AsArray());
//                    var ls = new List<EvaluationCommentDto>();
//                    var i = 0;
//                    foreach (var item in pg.CurrentPageItems)
//                    {
//                        if (cc == null || cc.Length <= i || cc[i] == null) ls.Add(item);
//                        else item.IsLikeByMe = cc[i].In("0", "null", "") ? false : true;  //Convert.ToInt32(cc[i]) > 0;
//                        i++;
//                    }
//                    if (!ls.Any()) break;

//                    sql = $@"
//select cmmt.id
//from EvaluationComment cmmt 
//left join (select commentid from [Like] 
//    where type={(2)} and evaluationid=@EvltId and useid=@UserId and commentid in @Ids
//    group by commentid
//) lk on lk.commentid=cmmt.id
//where cmmt.IsValid=1 and cmmt.id in @Ids and lk.commentid is not null
//";
//                    var likes = await unitOfWork.DbConnection.QueryAsync<Guid>(sql, new
//                    {
//                        Ids = ls.Select(_ => _.Id),
//                        req.EvltId,
//                        me.UserId,
//                    });
//                    if (likes?.Any() != true)
//                    {
//                        break;
//                    }
//                    foreach (var like in likes)
//                    {
//                        var item = ls.FirstOrDefault(_ => _.Id == like);
//                        if (item == null) continue;
//                        item.IsLikeByMe = true;
//                    }

//                    //var pipe = redis.StartPipe();
//                    //foreach (var item in ls)
//                    //    pipe.HSet(rdk, item.Id.ToString(), 1);
//                    //pipe.Expire(rdk, 60 * 60 * 6);
//                    //await pipe.EndPipeAsync();
//                    break;
//                }
                #endregion
            }

            if (tsk_findUser != null) await tsk_findUser;
            if (req.AllowFindChilds) await GetChildren(pg.CurrentPageItems);
            return pg;
        }

        async Task FindUserInfo(List<EvaluationCommentDto> lsU)
        {
            var ru = await mediator.Send(new UserSimpleInfoQuery { UserIds = lsU.Select(_ => _.UserId) });
            foreach (var u in lsU)
            {
                var r = ru.FirstOrDefault(_ => _.Id == u.UserId);
                if (r == null) r = new UserSimpleInfoQueryResult { Id = u.UserId };
                u.Username = r.Nickname;
                u.UserImg = r.HeadImgUrl ?? config["AppSettings:UserDefaultHeadImg"];
            }
        }
        async Task GetChildren(IEnumerable<EvaluationCommentDto> lsU)
        {

            foreach (var u in lsU)
            {
                //暂未加缓存
                var sql = "SELECT * from EvaluationComment where fromid=@fromid and userid=@userid and IsValid=1 ORDER BY CreateTime desc ";
                var subModel = await unitOfWork.QueryFirstOrDefaultAsync<SubCommentDto>(sql,
                new { fromid =u.Id, userid =u.AuthorId});
                if (null != subModel)
                {
                    subModel.IsAuthor = true;//只查作者的回复
                    u.SubComments = new SubCommentDto[] { subModel };
                }
            }
        }
    }
}
