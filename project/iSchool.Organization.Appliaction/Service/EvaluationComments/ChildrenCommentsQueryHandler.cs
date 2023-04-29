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
    public class ChildrenCommentsQueryHandler : IRequestHandler<ChildrenCommentsQuery, PagedList<EvaluationCommentDto>>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;
        IConfiguration config;

        public ChildrenCommentsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, CSRedisClient redis,
            IConfiguration config)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;
            this.config = config;
        }

        public async Task<PagedList<EvaluationCommentDto>> Handle(ChildrenCommentsQuery req, CancellationToken cancellation)
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
            if (pg.CurrentPageIndex == 1 && pg.PageSize == 10)
            {
                var rdk = CacheKeys.EvltCommentChildrendCommentTop10.FormatWith(req.EvltCommentId);
                var jtk = await redis.GetAsync<JToken>(rdk);
                if (null != jtk)
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
            sql = $@"select count(1) from EvaluationComment where IsValid=1 and fromid=@EvltCommentId

select id,userid,CreateTime,comment,(case when likes>=10 then likes else -1 end) _i,
username,likes,CommentCount
from  EvaluationComment
where  IsValid=1 and fromid=@EvltCommentId 
order by _i desc,CommentCount desc,CreateTime desc
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";    
            var gr = await unitOfWork.QueryMultipleAsync(sql,
                new DynamicParameters(req));
            pg.TotalItemCount = await gr.ReadFirstAsync<int>();
            pg.CurrentPageItems = await gr.ReadAsync<EvaluationCommentDto>();

            if (pg.CurrentPageIndex == 1 && pg.PageSize == 10)
            {
                var rdk = CacheKeys.EvltCommentChildrendCommentTop10.FormatWith(req.EvltCommentId);
                await redis.SetAsync(rdk, new { totalItemCount = pg.TotalItemCount, page1_items = pg.CurrentPageItems }, 60 * 15);
            }

            LB_user:
            do
            {
                var evaltModel = await unitOfWork.QueryFirstOrDefaultAsync<Evaluation>(@"select * from Evaluation where id = @id ", new { id = req.EvltId });
               
                var lsU = new List<EvaluationCommentDto>();
                foreach (var item in pg.CurrentPageItems)
                {
                    item.Now = now;
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
                    if(null!= evaltModel)
                    item.IsAuthor = item.UserId == evaltModel.Userid ? true : false;
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
                //do
                //{
                //    var rdk = CacheKeys.EvaluationCommentLikesCount.FormatWith(req.EvltId);
                //    var cc = await redis.HMGetAsync(rdk, pg.CurrentPageItems.Select(_ => _.Id.ToString()).ToArray());
                //    var ls = new List<EvaluationCommentDto>();
                //    var i = 0;
                //    foreach (var item in pg.CurrentPageItems)
                //    {
                //        var redisLikeCount = Convert.ToInt32(cc[i]);
                //        if (cc == null || cc.Length <= i || cc[i] == null) ls.Add(item);

                //        else item.Likes = redisLikeCount < 0 ? 0 : redisLikeCount;
                //        i++;
                //    }
                //    if (!ls.Any()) break;

                //    sql = @"select Id as Item1,likes as Item2 from EvaluationComment where IsValid=1 and id in @Ids";
                //    var likes = await unitOfWork.DbConnection.QueryAsync<(Guid, int)>(sql, new { Ids = ls.Select(_ => _.Id) });
                //    foreach (var like in likes)
                //    {
                //        var item = ls.FirstOrDefault(_ => _.Id == like.Item1);
                //        if (item == null) continue;
                //        item.Likes = like.Item2;
                //    }

                //    var pipe = redis.StartPipe();
                //    foreach (var item in ls)
                //        pipe.HSet(rdk, item.Id.ToString(), item.Likes);
                //    pipe.Expire(rdk, 60 * 60 * 6);
                //    await pipe.EndPipeAsync();
                //}
                //while (false);
                //// IsLikeByMe
                //while (me.IsAuthenticated)
                //{
                //    var rdk = CacheKeys.MyCommentLikes.FormatWith(me.UserId, req.EvltId);
                //    var cc = await redis.HMGetAsync(rdk, pg.CurrentPageItems.Select(_ => _.Id.ToString()).AsArray());
                //    var ls = new List<EvaluationCommentDto>();
                //    var i = 0;
                //    foreach (var item in pg.CurrentPageItems)
                //    {
                //        if (cc == null || cc.Length <= i || cc[i] == null) ls.Add(item);
                //        else item.IsLikeByMe = cc[i].In("0", "null", "") ? false : true;  //Convert.ToInt32(cc[i]) > 0;
                //        i++;
                //    }
                //    //                if (!ls.Any()) break;

                //    //                sql = $@"select cmmt.id from EvaluationComment cmmt 
                //    //left join (select commentid from [Like] 
                //    //    where type={(2)} and  useid=@UserId and commentid in @Ids
                //    //    group by commentid
                //    //) lk on lk.commentid=cmmt.id
                //    //where cmmt.IsValid=1 and cmmt.id in @Ids and lk.commentid is not null
                //    //";
                //    //                var likes = await unitOfWork.DbConnection.QueryAsync<Guid>(sql, new
                //    //                {
                //    //                    Ids = ls.Select(_ => _.Id),

                //    //                    me.UserId,
                //    //                });
                //    //                if (likes?.Any() != true)
                //    //                {
                //    //                    break;
                //    //                }
                //    //                foreach (var like in likes)
                //    //                {
                //    //                    var item = ls.FirstOrDefault(_ => _.Id == like);
                //    //                    if (item == null) continue;
                //    //                    item.IsLikeByMe = true;
                //    //                }

                //    //                var pipe = redis.StartPipe(); 
                //    //                foreach (var item in ls)
                //    //                    pipe.HSet(rdk, item.Id.ToString(), 1);
                //    //                pipe.Expire(rdk, 60 * 60 * 6);
                //    //                await pipe.EndPipeAsync();
                //    break;
                //}
                #endregion
            }

            if (tsk_findUser != null) await tsk_findUser;
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
    }
}
