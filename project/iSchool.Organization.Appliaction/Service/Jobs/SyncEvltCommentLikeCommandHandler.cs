using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class SyncEvltCommentLikeCommandHandler : IRequestHandler<SyncEvltCommentLikeCommand>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;

        public SyncEvltCommentLikeCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
        }

        public async Task<Unit> Handle(SyncEvltCommentLikeCommand cmd, CancellationToken cancellation)
        {
            var cancel = new CancellationTokenSource(cmd.Ttl);
            var ls = new List<(Guid UserId, Guid EvltId, Guid CommentId, DateTime CreateTime, bool IsLike)>();

            var cursor = 0L;
            while (!cancel.IsCancellationRequested)
            {
                try
                {
                    var scan = await redis.HScanAsync(CacheKeys.CommentLikeAction, cursor, "*", 2000);
                    cursor = scan.Cursor;
                    if (scan.Items.Length > 0)
                    {
                        var pipe = redis.StartPipe();
                        foreach (var (field, value) in scan.Items)
                        {
                            do
                            {
                                var regx = Regex.Match(field, @"^(?<EvltId>[0-9a-fA-F\-]+)\|(?<CommentId>[0-9a-fA-F\-]+)\|(?<UserId>[0-9a-fA-F\-]+)$").Groups;
                                var evltId = Guid.TryParse(regx["EvltId"].Value, out var _evltId) ? _evltId : default;
                                var commentId = Guid.TryParse(regx["CommentId"].Value, out var _commentId) ? _commentId : default;
                                var uid = Guid.TryParse(regx["UserId"].Value, out var _uid) ? _uid : default;
                                if (evltId == default || commentId == default || uid == default) break;
                                var i = value.IndexOf('-');
                                if (i == -1) break;
                                var ct = DateTime.UnixEpoch.AddMilliseconds(Convert.ToInt64(value[0..i])).ToLocalTime();
                                var isL = value[(i + 1)..] == "1";
                                ls.Add((uid, evltId, commentId, ct, isL));
                            }
                            while (false);
                            pipe.HDel(CacheKeys.CommentLikeAction, field);
                        }
                        await pipe.EndPipeAsync();
                    }
                    else if (scan.Cursor <= 0)
                    {                        
                        break;
                    }
                }
                catch (Exception ex)
                {
                    //log.LogError(ex, "");
                }
            }

            if (!ls.Any()) return default;

            ls = ls.OrderBy(_ => _.CreateTime).ToList();
            await Dosync1(ls);
            await DosyncFrom_cache(ls);

            return default;
        }

        async Task Dosync1(List<(Guid UserId, Guid EvltId, Guid CommentId, DateTime CreateTime, bool IsLike)> ls)
        {
            // IsLike=true 插入不存在
            // IsLike=false 删除比此CreateTime早的
            //
            var sql = @"
if @IsLike = 1 begin 
insert [Like](id,type,evaluationid,commentid,useid,CreateTime)
    select newid(),2,@EvltId,@CommentId,@UserId,@CreateTime 
    where not exists(select 1 from [Like] where type=2 and useid=@UserId and evaluationid=@EvltId and commentid=@CommentId) 
end else begin
delete from [Like] where type=2 and useid=@UserId and evaluationid=@EvltId and commentid=@CommentId and CreateTime<@CreateTime
end";
            try
            {
                unitOfWork.BeginTransaction();

                var dyp = ls.Select(_ => new { _.UserId, _.EvltId, _.CommentId, _.CreateTime, _.IsLike });
                await unitOfWork.DbConnection.ExecuteAsync(sql, dyp, unitOfWork.DbTransaction);

                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                try { unitOfWork.Rollback(); } catch { }
                throw ex;
            }            
        }

        async Task DosyncFrom_dboLike(IEnumerable<Guid> commentIds)
        {
            var sql = @"
update e set e.likes=c.cc
from EvaluationComment e, (select commentid,count(1) as cc from [Like] where type=2 and commentid in @commentIds group by commentid)c
where e.IsValid=1 and e.id=c.commentid and e.id in @commentIds
";
            await unitOfWork.DbConnection.ExecuteAsync(sql, new { commentIds });
        }

        async Task DosyncFrom_cache(List<(Guid UserId, Guid EvltId, Guid CommentId, DateTime CreateTime, bool IsLike)> ls)
        {
            var elk = new List<(Guid EvltId, Guid CommentId, int? Likecount)>();
            foreach (var arr in SplitArr(ls.Select(_ => (_.EvltId, _.CommentId)).Distinct(), 5))
            {
                var pipe = redis.StartPipe();
                Array.ForEach(arr, a => pipe.HGet(CacheKeys.EvaluationCommentLikesCount.FormatWith(a.EvltId), $"{a.CommentId}"));
                var rr = await pipe.EndPipeAsync();

                for (var i = 0; i < arr.Length; i++)
                {
                    elk.Add((arr[i].EvltId, arr[i].CommentId, int.TryParse(rr[i]?.ToString(), out var _c) ? _c : (int?)null));
                }
            }

            // cache失效 直接从Like表更新
            await DosyncFrom_dboLike(elk.Where(_ => _.Likecount == null).Select(_ => _.CommentId));

            // 从cache更新 
            //
            var sql = @"
update dbo.EvaluationComment set likes=@Likecount where id=@CommentId
";
            await unitOfWork.DbConnection.ExecuteAsync(sql, elk.Where(_ => _.Likecount != null).Select(_ => new
            {
                _.CommentId,
                _.Likecount,
            }));
        }

        static IEnumerable<T[]> SplitArr<T>(IEnumerable<T> collection, int c /* c > 0 */)
        {
            for (var arr = collection; arr.Any();)
            {
                yield return arr.Take(c).ToArray();
                arr = arr.Skip(c);
            }
        }
    }
}
