using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
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
    public class SyncPVCommandHandler : IRequestHandler<SyncPVCommand>
    {
        OrgUnitOfWork _unitOfWork;
        IMediator mediator;
        CSRedisClient redis;

        public SyncPVCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
        }

        public async Task<Unit> Handle(SyncPVCommand cmd, CancellationToken cancellation)
        {
            var date = (cmd.Time ?? DateTime.Now).Date;

            // clear old pv
            {
                var sql = "delete from PV where [date]<=@date";
                await _unitOfWork.ExecuteAsync(sql, new { date = DateTime.Now.AddDays(-20).Date });
            }

            await foreach (var ls in GetKeys(cmd))
            {
                if (ls.Count < 1) continue;
                using var pipe = redis.StartPipe();
                foreach (var x in ls)
                {
                    pipe.Get(CacheKeys.PV_total.FormatWith(
                        ("date", date.ToString("yyyy-MM-dd")), ("ctt", x.Item1), ("cttid", x.Item2)
                    ));
                }
                var rr = await pipe.EndPipeAsync();

                var pvs = FromRange(0, ls.Count - 1).Select(i =>
                {
                    var vc = int.TryParse(rr[i]?.ToString(), out var _i) ? _i : -1;
                    return vc < 0 ? null : new PV
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        IsValid = true,
                        Contentid = ls[i].Item2,
                        Contenttype = (byte)ls[i].Item1,
                        Viewcount = vc,
                    };
                }).Where(_ => _ != null);

                var sql = @"
delete from PV where date=@date and Contenttype=@Contenttype and Contentid=@Contentid and IsValid=1 ;
insert PV(id,date,Contenttype,Contentid,viewcount,IsValid)
    values(@Id,@Date,@Contenttype,@Contentid,@Viewcount,1) ;
";
                await _unitOfWork.DbConnection.ExecuteAsync(sql, pvs);

                foreach (var (enm, _) in EnumUtil.GetDescs<PVisitCttTypeEnum>())
                {
                    var pvs1 = pvs.Where(x => x.Contenttype == (byte)enm);
                    if (!pvs1.Any()) continue;
                    switch (enm)
                    {
                        case PVisitCttTypeEnum.Evaluation:
                            {
                                sql = $@"
select ContentType,contentid,sum(isnull(viewcount,0)) cc 
into #T
from PV with(nolock)
    where IsValid=1 and (DATEDIFF(dd,[date],getdate()) between 0 and {(7 - 1)}) and ContentType={PVisitCttTypeEnum.Evaluation.ToInt()} and contentid in @ids
    group by ContentType,contentid

update e set e.viewcount=c.cc
from Evaluation e, #T c
where e.IsValid=1 and e.id=c.contentid and e.id in @ids

drop table #T
";
                                await _unitOfWork.DbConnection.ExecuteAsync(sql, new { ids = pvs1.Select(_ => _.Contentid) });
                            }
                            break;
                        case PVisitCttTypeEnum.Course:
                            {
                                sql = $@"
select ContentType,contentid,sum(isnull(viewcount,0)) cc 
into #T
from PV with(nolock)
    where IsValid=1 and (DATEDIFF(dd,[date],getdate()) between 0 and {(7 - 1)}) and ContentType={PVisitCttTypeEnum.Course.ToInt()} and contentid in @ids
    group by ContentType,contentid

update c0 set c0.viewcount=c.cc
from Course c0, #T c
where c0.id=c.contentid and c0.id in @ids

drop table #T
";
                                await _unitOfWork.DbConnection.ExecuteAsync(sql, new { ids = pvs1.Select(_ => _.Contentid) });
                            }
                            break;
                        case PVisitCttTypeEnum.Organization:
                            { 
                                //...待定
                            }
                            break;
                    }
                }
            }
            return default;
        }

        async IAsyncEnumerable<IList<(PVisitCttTypeEnum, Guid)>> GetKeys(SyncPVCommand cmd)
        {
            var ls = new List<(PVisitCttTypeEnum, Guid)>();
            var kps = new[] { CacheKeys.PV_diffAll };
            await foreach (var keys in Split(redis.ScanKeys(kps), 100))
            {
                // org:pv:diff:{ctt}_{guid}
                foreach (var key in keys)
                {
                    var id = key.Length > 37 && Guid.TryParse(key[^36..], out var _gid) ? _gid : default;
                    if (id == default) continue;
                    var enm = key[(key.LastIndexOf(':') + 1)..^37].ToEnum<PVisitCttTypeEnum>();
                    ls.Add((enm, id));
                }
                yield return ls;
                ls.Clear();
                await redis.BatchDelAsync(keys);
            }
        }        

        static async IAsyncEnumerable<IEnumerable<T>> Split<T>(IAsyncEnumerable<T> arr, int len)
        {
            List<T> ls = null;
            await foreach (var k in arr)
            {
                ls ??= new List<T>(len);
                ls.Add(k);
                if (ls.Count < len) continue;
                yield return ls;
                ls.Clear();
            }
            if (ls?.Count > 0)
            {
                yield return ls;
            }
        }

        static IEnumerable<int> FromRange(int start, int end) => Enumerable.Range(start, end + 1 - start);
    }
}
