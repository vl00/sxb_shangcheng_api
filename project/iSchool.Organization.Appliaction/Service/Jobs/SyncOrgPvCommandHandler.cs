using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Modles;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.CommonHelper;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Service
{
    public class SyncOrgPvCommandHandler : IRequestHandler<SyncOrgPvCommand>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator mediator;
        CSRedisClient redis;
        IHttpClientFactory _httpClientFactory;
        IConfiguration _config;
        NLog.ILogger _log;

        public SyncOrgPvCommandHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IHttpClientFactory httpClientFactory, IConfiguration config,
            NLog.ILogger log)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.redis = redis;
            _httpClientFactory = httpClientFactory;
            _log = log;
            _config = config;
        }

        public async Task<Unit> Handle(SyncOrgPvCommand cmd, CancellationToken cancellation)
        {
            var date = (cmd.Time ?? DateTime.Now).Date;            

            using var http = _httpClientFactory.CreateClient(string.Empty);
            var url = $"{_config["AppSettings:pv_url"]}/orgpv";

            var rr = await new HttpApiInvocation(HttpMethod.Post, url, _log)
                .SetApiDesc("get机构pv")
                .SetBodyByForm(new Dictionary<string, object> { ["timestamp"] = new DateTimeOffset(date).ToUnixTimeMilliseconds() })
                .SetResBodyParser((resStr) =>
                {
                    var jkn = JToken.Parse(resStr);
                    if ((int)jkn["status"] != 0) return ResponseResult<_Org_PV[]>.Failed(jkn["errorDescription"]?.ToString());
                    return ResponseResult<_Org_PV[]>.Success(jkn["items"].ToObject<_Org_PV[]>());
                })
                .InvokeByAsync<_Org_PV[]>(http);

            if (!rr.Succeed) return default;

            // 更新pv
            var has_pv = false;
            foreach (var items in SplitArr(rr.Data, 20))
            {
                var sql = @"select [no],id from Organization where [no] in @no";
                var ids = await _orgUnitOfWork.QueryAsync<(long, Guid)>(sql, new { no = items.Select(_ => _.Id) });

                var pvs = (
                    from item in items
                    join id in ids on item.Id equals id.Item1 into iii
                    from ii in iii.DefaultIfEmpty()
                    select new { OrgId = ii.Item2, item.Pv }
                ).Where(_ => _.OrgId != default);

                if (!pvs.Any()) continue;
                has_pv = true;

                sql = @"insert Pv4Org(Id,OrgId,Viewcount,IsValid,[Time]) select newid(),@OrgId,@Pv,0,null";
                await _orgUnitOfWork.ExecuteAsync(sql, pvs);
            }
            if (has_pv)
            {
                var sql = @"
update Pv4Org set IsValid=0 where IsValid=1
update Pv4Org set IsValid=1,[Time]=getdate() where IsValid=0 and [Time] is null
delete from Pv4Org where IsValid=0 and datediff(dd,[Time],getdate())>1
";
                await _orgUnitOfWork.ExecuteAsync(sql);
            }

            return default;
        }

        class _Org_PV
        {
            public long Id { get; set; }
            public int Pv { get; set; }
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
