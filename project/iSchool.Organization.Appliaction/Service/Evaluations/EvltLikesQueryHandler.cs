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
    public class EvltLikesQueryHandler : IRequestHandler<EvltLikesQuery, EvltLikesQueryResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;              

        public EvltLikesQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, IUserInfo me, 
            CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;            
        }

        public async Task<EvltLikesQueryResult> Handle(EvltLikesQuery query, CancellationToken cancellation)
        {
            var result = new EvltLikesQueryResult();            
            var itemDict = result.Items = new Dictionary<Guid, (int Likecount, bool IsLikeByMe)>();

            await default(ValueTask);

            if (query.EvltIds?.Any() != true) return result;

            var ls_notIn = new List<Guid>(); // not in caches
            string sql = null;

            // 
            // find likecount
            //
            if (true)
            {
                using var pipe = redis.StartPipe();
                foreach (var id in query.EvltIds)
                {
                    pipe.HGetAll(CacheKeys.EvaluationLikesCount.FormatWith(id));
                }
                var rr = await pipe.EndPipeAsync();
                for (var i = 0; i < rr.Length; i++)
                {
                    int ib = -1, ib1 = -1;
                    if (rr[i] is Dictionary<string, string> dict)
                    {
                        // cache里同时有 like和shamlikes 才算有效
                        ib = int.Parse(dict.GetValueEx("like", "-1"));
                        ib1 = int.Parse(dict.GetValueEx("shamlikes", "-1"));
                        itemDict[query.EvltIds[i]] = (Math.Max(0, ib) + (ib == -1 || ib1 == -1 ? 0 : ib1), default);
                    }
                    else itemDict[query.EvltIds[i]] = (0, default);
                    if (ib == -1 || ib1 == -1) ls_notIn.Add(query.EvltIds[i]);
                }
            }
            // 查db查出不在cache的
            if (ls_notIn.Count > 0)
            {
                sql = $@"
select e.Id as Item1,e.Likes as Item2,isnull(e.Shamlikes,0) as Item3
from Evaluation e
where e.IsValid=1 and e.status={EvaluationStatusEnum.Ok.ToInt()}
and e.id in @evltIds
";
                var dy = await unitOfWork.QueryAsync<(Guid, int, int)>(sql, new { evltIds = ls_notIn });
                var dy2 = new Dictionary<Guid, (int, int)>();
                foreach (var item in dy)
                {
                    if (!itemDict.TryGetValue(item.Item1, out var v)) continue;
                    dy2[item.Item1] = ((v.Likecount > 0 ? v.Likecount : item.Item2), item.Item3);
                    v.Likecount = (v.Likecount > 0 ? v.Likecount : item.Item2) + item.Item3;                    
                    itemDict[item.Item1] = v;
                }
                var expls = new List<string>();
                {
                    using var pipe = redis.StartPipe();
                    foreach (var id in dy2.Keys) //ls_notIn
                    {
                        pipe.Ttl(CacheKeys.EvaluationLikesCount.FormatWith(id));
                        pipe.HSetNx(CacheKeys.EvaluationLikesCount.FormatWith(id), "like", dy2[id].Item1);
                        pipe.HSet(CacheKeys.EvaluationLikesCount.FormatWith(id), "shamlikes", dy2[id].Item2);
                    }
                    var rr = await pipe.EndPipeAsync();
                    for (var i = 0; i < rr.Length; i += 3)
                    {
                        if (rr[i] is -1L || rr[i] is -2L)
                            expls.Add(CacheKeys.EvaluationLikesCount.FormatWith(ls_notIn[i / 3]));
                    }
                }
                if (expls.Count > 0)
                {
                    var pipe = redis.StartPipe();
                    foreach (var k in expls)
                        pipe.Expire(k, 60 * 60 * 24 * 1);
                    _ = pipe.EndPipeAsync();
                }
            }
            ls_notIn.Clear();

            // 是否'我点赞过的'需要已登录验证
            if (!me.IsAuthenticated)
            {
                return result;
            }

            //
            // find islikebyme
            //
            // 查主cache
            if (true) 
            {
                var rdkLike = CacheKeys.MyEvaluationLikes.FormatWith(me.UserId);
                using var pipe = redis.StartPipe();
                foreach (var id in query.EvltIds)
                {
                    pipe.HGet(rdkLike, id.ToString());
                }
                var rr = await pipe.EndPipeAsync();
                for (var i = 0; i < rr.Length; i++)
                {
                    var _rr = rr[i]?.ToString();
                    if (_rr.IsNullOrEmpty()) ls_notIn.Add(query.EvltIds[i]);
                    else
                    {
                        var v = itemDict[query.EvltIds[i]];
                        v.IsLikeByMe = !_rr.IsNullOrEmpty();
                        itemDict[query.EvltIds[i]] = v;
                    }
                }
            }
            // 查点赞记录cache查出不在主cache的
            if (ls_notIn.Count > 0)
            {
                using var pipe = redis.StartPipe();
                foreach (var id in ls_notIn)
                {
                    pipe.HGet(CacheKeys.EvaluationLikeAction, $"{id}|{me.UserId}");
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
            // 查db查出不在cache的
            if (ls_notIn.Count > 0)
            {
                sql = $@"
select evaluationid as Item1,0 as Item2,CreateTime as Item3 from [Like] where type={(1)} and useid=@UserId and evaluationid in @evltIds
";
                var dy = await unitOfWork.QueryAsync<(Guid, int, string)>(sql, new { me.UserId, evltIds = ls_notIn });
                foreach (var item in dy)
                {
                    if (!itemDict.TryGetValue(item.Item1, out var v)) continue;
                    v.IsLikeByMe = !item.Item3.IsNullOrEmpty();
                    itemDict[item.Item1] = v;
                }                
                // set to redis
                using var pipe = redis.StartPipe();
                foreach (var id in ls_notIn)
                {                    
                    if (itemDict[id].IsLikeByMe)
                        pipe.HSetNx(CacheKeys.MyEvaluationLikes.FormatWith(me.UserId), $"{id}", "2020-01-01");
                    //else
                    //    pipe.HSetNx(CacheKeys.EvaluationLikeAction, $"{id}|{me.UserId}", $"100-{(itemDict[id].IsLikeByMe ? 1 : 0)}");
                }
                await pipe.EndPipeAsync();
            }
            ls_notIn.Clear();
            
            return result;
        }

    }
}
