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
    public class SyncEvltUVCommandHandler : IRequestHandler<SyncEvltUVCommand>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        CSRedisClient redis;

        public SyncEvltUVCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
        }

        public async Task<Unit> Handle(SyncEvltUVCommand cmd, CancellationToken cancellation)
        {
            var date = (cmd.Time ?? DateTime.Now).Date;
            var ls = new List<(DateTime, Guid, int)>();
            var sql = string.Empty;
            await default(ValueTask);

            await foreach (var key in GetKeys(cmd))
            {
                var evltId = key.Length > 37 && Guid.TryParse(key[^36..], out var _gid) ? _gid : default;
                if (evltId == default) continue;

                var rdk = CacheKeys.UV_evlt_total.FormatWith(("date", date.ToString("yyyy-MM-dd")), ("evltid", evltId));
                var count = await redis.GetAsync<int?>(rdk);
                ls.RemoveAll(_ => _.Item2 == evltId);
                ls.Add((date, evltId, count ?? 0));

                sql = @"
delete from UV4Evaluation where date=@date and evaluationid=@evltId and IsValid=1 ;
insert UV4Evaluation(id,date,evaluationid,viewcount,IsValid)
    values(newid(),@date,@evltId,@count,1) ;
";
                await unitOfWork.DbConnection.ExecuteAsync(sql, new { date, evltId, count });
            }
            if (!ls.Any()) return default;

            foreach (var eids in each_Ls(ls))
            {
                sql = $@"
update e set e.viewcount=c.cc
from Evaluation e, (select evaluationid,sum(isnull(viewcount,0)) cc from UV4Evaluation with(nolock)
    where IsValid=1 and (DATEDIFF(dd, [date], getdate()) between 0 and {(7 - 1)}) and evaluationid in @evltIds
    group by evaluationid )c
where e.IsValid=1 and e.id=c.evaluationid and e.id in @evltIds
";
                await unitOfWork.DbConnection.ExecuteAsync(sql, new { evltIds = eids.Where(_ => _ != default) });

                var pipe = redis.StartPipe();
                foreach (var eid in eids)                
                    pipe.HDel(CacheKeys.EvaluationLikesCount.FormatWith(eid), "viewer");                
                await pipe.EndPipeAsync();
            }
            return default;
        }

        async IAsyncEnumerable<string> GetKeys(SyncEvltUVCommand cmd)
        {
            var date = (cmd.Time ?? DateTime.Now).Date;
            var cursor = 0L;
            var rdk_diff = CacheKeys.UV_evlt_diff.FormatWith(("date", date.ToString("yyyy-MM-dd")), ("evltid", "*"));
            while (true)
            {
                var scan = await redis.ScanAsync(cursor, rdk_diff, 2000);
                cursor = scan.Cursor;
                if (scan.Items.Length > 0)
                {
                    foreach (var k in scan.Items)
                    {
                        yield return k;
                        await redis.DelAsync(k);
                    }
                }
                else if (scan.Cursor <= 0)
                {
                    break;
                }
            }
        }

        IEnumerable<Guid[]> each_Ls(List<(DateTime, Guid, int)> ls, int c = 5)
        {
            var len = ls.Count;
            while (len > 0)
            {
                var gids = new Guid[c];
                for (var i = 0; i < c; i++)
                {
                    gids ??= new Guid[c];
                    gids[i] = ls[0].Item2;
                    ls.RemoveAt(0);
                    if (--len < 1) break;
                }
                yield return gids;
            }
        }
    }
}
