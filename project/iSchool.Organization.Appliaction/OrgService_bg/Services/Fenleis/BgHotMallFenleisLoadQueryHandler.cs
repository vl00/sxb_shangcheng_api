using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.OrgService_bg.RequestModels;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services
{
    public class BgHotMallFenleisLoadQueryHandler : IRequestHandler<BgHotMallFenleisLoadQuery, BgHotMallFenleisLoadQueryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public BgHotMallFenleisLoadQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<BgHotMallFenleisLoadQueryResult> Handle(BgHotMallFenleisLoadQuery query, CancellationToken cancellation)
        {
            var ls = new List<BgMallFenleisLoadQueryResult>(3) { null, null, null };
            var result = new BgHotMallFenleisLoadQueryResult { Ls = ls };
            await default(ValueTask);

            var sql = $@"
select kv.[key] as code,kv.name,p.sort
from PopularClassify p left join KeyValue kv on p.classifykey=kv.[key]
where p.IsValid=1 and kv.depth={3} and kv.IsValid=1 and kv.type={Consts.Kvty_MallFenlei}
order by p.sort
";
            var als = (await _orgUnitOfWork.QueryAsync<(int, string, int)>(sql)).AsList();
            if (!als.Any()) goto LB_end;

            for (var i = 0; i < als.Count; i++)
            {
                var (code, _, sort) = als[i];
                if (!sort.In(1, 2, 3)) continue;
                if (ls[sort - 1] != null) continue;

                ls[sort - 1] = await _mediator.Send(new BgMallFenleisLoadQuery { Code = code, ExpandMode = 2 });                
            }

            LB_end:
            IEnumerable<BgMallFenleiItemDto> d1s = null;
            for (var i = 0; i < ls.Count; i++)
            {
                if (ls[i] != null) continue;
                ls[i] = new BgMallFenleisLoadQueryResult();
                ls[i].D1s = (d1s ??= (await _mediator.Send(new BgMallFenleisLoadQuery { Code = 0, ExpandMode = 1 })).D1s);
            }
            return result;
        }

    }
}
