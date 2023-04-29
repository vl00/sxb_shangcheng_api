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
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class SyncEvltLikeCommandHandler : IRequestHandler<SyncEvltLikeCommand>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;

        public SyncEvltLikeCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
        }

        public async Task<Unit> Handle(SyncEvltLikeCommand cmd, CancellationToken cancellation)
        {
            var cancel = new CancellationTokenSource(cmd.Ttl);
            var ls = new List<(Guid UserId, Guid EvltId, DateTime CreateTime, bool IsLike)>();

            var cursor = 0L;
            while (!cancel.IsCancellationRequested)
            {
                try
                {
                    var scan = await redis.HScanAsync(CacheKeys.EvaluationLikeAction, cursor, "*", 2000);
                    cursor = scan.Cursor;
                    if (scan.Items.Length > 0)
                    {
                        var pipe = redis.StartPipe();
                        foreach (var (field, value) in scan.Items)
                        {
                            do
                            {
                                var i = field.IndexOf('|');
                                if (i == -1) break;
                                var evltId = field[0..i];
                                var uid = field[(i + 1)..];
                                i = value.IndexOf('-');
                                if (i == -1) break;
                                var ct = DateTime.UnixEpoch.AddMilliseconds(Convert.ToInt64(value[0..i])).ToLocalTime();
                                var isL = value[(i + 1)..] == "1";
                                ls.Add((Guid.Parse(uid), Guid.Parse(evltId), ct, isL));
                            }
                            while (false);
                            pipe.HDel(CacheKeys.EvaluationLikeAction, field);
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

        async Task Dosync1(List<(Guid UserId, Guid EvltId, DateTime CreateTime, bool IsLike)> ls)
        {
            // IsLike=true 插入不存在
            // IsLike=false 删除比此CreateTime早的
            //
            var sql = @"
if @IsLike = 1 begin 
insert [Like](id,type,evaluationid,commentid,useid,CreateTime)
    select newid(),1,@EvltId,null,@UserId,@CreateTime 
    where not exists(select 1 from [Like] where type=1 and useid=@UserId and evaluationid=@EvltId ) 
end else begin
delete from [Like] where type=1 and useid=@UserId and evaluationid=@EvltId and CreateTime<@CreateTime
end";
            try
            {
                unitOfWork.BeginTransaction();

                var dyp = ls.Select(_ => new { _.UserId, _.EvltId, _.CreateTime, _.IsLike });
                await unitOfWork.DbConnection.ExecuteAsync(sql, dyp, unitOfWork.DbTransaction);

                unitOfWork.CommitChanges();
            }
            catch (Exception ex)
            {
                try { unitOfWork.Rollback(); } catch { }
                throw ex;
            }            
        }

        async Task DosyncFrom_dboLike(IEnumerable<Guid> evltIds)
        {
            var sql = @"
update e set e.likes=c.cc
from Evaluation e, (select evaluationid,count(1) as cc from [Like] where type=1 and evaluationid in @evltIds group by evaluationid)c
where e.id=c.evaluationid and e.id in @evltIds
";
            await unitOfWork.DbConnection.ExecuteAsync(sql, new { evltIds });
        }

        async Task DosyncFrom_cache(List<(Guid UserId, Guid EvltId, DateTime CreateTime, bool IsLike)> ls)
        {
            var elk = new List<(Guid EvltId, int? Likecount)>();
            foreach (var arr in SplitArr(ls.Select(_ => _.EvltId).Distinct(), 5))
            {
                var pipe = redis.StartPipe();
                Array.ForEach(arr, a => pipe.HGet(CacheKeys.EvaluationLikesCount.FormatWith(a), "like"));
                var rr = await pipe.EndPipeAsync();

                for (var i = 0; i < arr.Length; i++)
                {
                    elk.Add((arr[i], int.TryParse(rr[i]?.ToString(), out var _c) ? _c : (int?)null));
                }
            }

            // cache失效 直接从Like表更新
            await DosyncFrom_dboLike(elk.Where(_ => _.Likecount == null).Select(_ => _.EvltId));

            // 从cache更新 
            //
            var sql = @"
update dbo.Evaluation set likes=@Likecount where id=@EvltId
";
            await unitOfWork.DbConnection.ExecuteAsync(sql, elk.Where(_ => _.Likecount != null).Select(_ => new
            {
                _.EvltId,
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
