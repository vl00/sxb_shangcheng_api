using AutoMapper;
using CSRedis;
using Dapper;
using iSchool;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class EvltCommentLikesQueryHandler : IRequestHandler<EvltCommentLikesQuery, EvltCommentLikesQueryResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;              

        public EvltCommentLikesQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;            
        }

        public async Task<EvltCommentLikesQueryResult> Handle(EvltCommentLikesQuery query, CancellationToken cancellation)
        {
            var result = new EvltCommentLikesQueryResult();            
            var itemDict = result.Items = new Dictionary<(Guid EvltId, Guid CommentId), (int Likecount, bool IsLikeByMe)>();
            await default(ValueTask);

            if (query.Ids?.Any() != true) return result;

            var ls_notIn = new List<(Guid EvltId, Guid CommentId)>();
            string sql = null;

            // 
            // find likecount
            //
            if (true)
            {
                using var pipe = redis.StartPipe();
                foreach (var id in query.Ids)
                {
                    pipe.HGet(CacheKeys.EvaluationCommentLikesCount.FormatWith(id.EvltId), $"{id.CommentId}");
                }
                var rr = await pipe.EndPipeAsync();
                for (var i = 0; i < rr.Length; i++)
                {
                    var r = int.TryParse(rr[i]?.ToString(), out var _r) ? _r : -1;
                    if (r == -1) ls_notIn.Add(query.Ids[i]);
                    itemDict[query.Ids[i]] = (r == -1 ? 0 : r, default);
                }
            }
            if (ls_notIn.Count > 0)
            {
                sql = @"select Id as Item1,likes as Item2 from EvaluationComment where IsValid=1 and id in @Ids";
                var dy = await unitOfWork.QueryAsync<(Guid, int)>(sql, new { Ids = ls_notIn.Select(_ => _.CommentId) });
                foreach (var item in dy)
                {                    
                    if (!itemDict.TryGetOne(out var v0, (_) => _.Key.CommentId == item.Item1)) continue;
                    var v = v0.Value;
                    v.Likecount = item.Item2;
                    itemDict[v0.Key] = v;
                }
                var dy2 = dy.ToDictionary(_ => _.Item1, _ => _.Item2);
                var expls = new HashSet<string>();
                {
                    using var pipe = redis.StartPipe();
                    foreach (var id in ls_notIn)
                    {
                        pipe.Ttl(CacheKeys.EvaluationCommentLikesCount.FormatWith(id.EvltId));
                        pipe.HSet(CacheKeys.EvaluationCommentLikesCount.FormatWith(id.EvltId), $"{id.CommentId}", dy2[id.CommentId]);                        
                    }
                    var rr = await pipe.EndPipeAsync();
                    for (var i = 0; i < rr.Length; i += 2)
                    {
                        if (rr[i] is -1L || rr[i] is -2L)
                            expls.Add(CacheKeys.EvaluationCommentLikesCount.FormatWith(ls_notIn[i / 2]));
                    }
                }
                if (expls.Count > 0)
                {
                    var pipe = redis.StartPipe();
                    foreach (var k in expls)
                        pipe.Expire(k, 60 * 60 * 6);
                    _ = pipe.EndPipeAsync();
                }
            }
            ls_notIn.Clear();

            if (!me.IsAuthenticated)
            {
                return result;
            }

            //
            // find islikebyme
            //
            if (true) 
            {                
                using var pipe = redis.StartPipe();
                foreach (var id in query.Ids)
                {
                    pipe.HGet(CacheKeys.MyCommentLikes.FormatWith(me.UserId, id.EvltId), id.CommentId.ToString());
                }
                var rr = await pipe.EndPipeAsync();
                for (var i = 0; i < rr.Length; i++)
                {
                    var _rr = rr[i]?.ToString();
                    if (_rr.IsNullOrEmpty()) ls_notIn.Add(query.Ids[i]);
                    else
                    {
                        var v = itemDict[query.Ids[i]];
                        v.IsLikeByMe = !_rr.IsNullOrEmpty();
                        itemDict[query.Ids[i]] = v;
                    }
                }
            }
            if (ls_notIn.Any())
            {
                using var pipe = redis.StartPipe();
                foreach (var id in ls_notIn)
                {
                    pipe.HGet(CacheKeys.CommentLikeAction, $"{id.EvltId}|{id.CommentId}|{me.UserId}");
                }
                var rr = await pipe.EndPipeAsync();
                Action delAct = null;
                for (var i = 0; i < rr.Length; i++)
                {
                    var id = ls_notIn[i];
                    var ii = rr[i]?.ToString();
                    if (!ii.IsNullOrEmpty() && ii[^2] == '-')
                    {
                        var v = itemDict[id];
                        v.IsLikeByMe = ii[^1] == '1';
                        itemDict[id] = v;
                        delAct += () => ls_notIn.Remove(id);
                    }
                }
                delAct?.Invoke();
            }
            if (ls_notIn.Any())
            {
                sql = $@"
select cmmt.evaluationid as Item1,cmmt.id as Item2
from EvaluationComment cmmt 
left join (select commentid from [Like] 
    where type={(2)} and useid='{me.UserId}' 
    and ({string.Join(" or ", ls_notIn.Select(x => $"(evaluationid='{x.EvltId}' and commentid='{x.CommentId}')"))})
    group by commentid
) lk on lk.commentid=cmmt.id
where cmmt.IsValid=1 and lk.commentid is not null 
and ({string.Join(" or ", ls_notIn.Select(x => $"(cmmt.evaluationid='{x.EvltId}' and cmmt.id='{x.CommentId}')"))})
";
                var dy = await unitOfWork.QueryAsync<(Guid, Guid)>(sql);
                foreach (var item in dy)
                {
                    if (!itemDict.TryGetValue(item, out var v)) continue;
                    v.IsLikeByMe = true;
                    itemDict[item] = v;
                }                
                // set to redis
                using var pipe = redis.StartPipe();
                foreach (var id in ls_notIn)
                {        
                    if (itemDict[id].IsLikeByMe)
                        pipe.HSetNx(CacheKeys.MyCommentLikes.FormatWith(me.UserId, id.EvltId), $"{id.CommentId}", "2020-01-01");
                    //else
                    //    pipe.HSetNx(CacheKeys.CommentLikeAction, $"{id.EvltId}|{id.CommentId}|{me.UserId}", $"100-{(itemDict[id].IsLikeByMe ? 1 : 0)}");
                }
                await pipe.EndPipeAsync();
            }
            ls_notIn.Clear();
            
            return result;
        }

    }
}
