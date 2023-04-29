using AutoMapper;
using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.Service.KeyValues;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class PcOrgIndexQueryHandler : IRequestHandler<PcOrgIndexQuery, PcOrgIndexQueryResult>
    {
        OrgUnitOfWork unitOfWork;
        IMediator mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper mapper;
        IConfiguration config;

        public PcOrgIndexQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IUserInfo me, IConfiguration config,
            IMapper mapper)
        {
            this.unitOfWork = (OrgUnitOfWork)unitOfWork;
            this.mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this.mapper = mapper;
            this.config = config;
        }

        public async Task<PcOrgIndexQueryResult> Handle(PcOrgIndexQuery query, CancellationToken cancellation)
        {
            var result = new PcOrgIndexQueryResult();            
            result.Me = me.IsAuthenticated ? me : null;
            result.AllOrgTypes = GetMainSubj(true).Select(_ => new SelectItemsKeyValues { Key = _, Value = EnumUtil.GetDesc((OrgCfyEnum)_) }).AsList();

            result.PageInfo = await GetPagedList(query, result.AllOrgTypes);

            return result;
        }

        async Task<PagedList<PcOrgItemDto>> GetPagedList(PcOrgIndexQuery query, IEnumerable<SelectItemsKeyValues> cfys)
        {
            IEnumerable<PcOrgItemDto> items = null;
            int cc = 0;
            await default(ValueTask);
            cfys = cfys.Where(_ => _.Key != OrgCfyEnum.Other.ToInt());

            var rdk = CacheKeys.PC_OrgIndexLs.FormatWith(query.PageSize, query.Type,
                query.Authentication switch { true => 1, false => 2, _ => "" });

            if (query.PageIndex == 1)
            {
                var jtk = await redis.GetAsync<JToken>(rdk);
                if (jtk != null)
                {
                    cc = (int)jtk["totalItemCount"];
                    items = jtk["page1_items"].ToObject<PcOrgItemDto[]>();
                }
            }
            if (items == null) // find in db sql
            {
                var sql = $@"
select distinct o.id,o.name,o.logo,o.authentication,o.[desc],o.subdesc,o.types,o.subjects,o.CreateTime,o.no --,isnull(o.Hot_score,0)as Hot_score
from Organization o
outer apply openjson(o.types) with([type] int '$') j
where o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} {{0}}
{(query.Authentication == null ? "" : $"and o.authentication={(query.Authentication == true ? 1 : 0)}")}
";
                var sql_where = "";
                if (query.Type != null)
                {
                    var is_other = query.Type.Value.In(-1, OrgCfyEnum.Other.ToInt())
                        || !query.Type.Value.In(cfys.Select(_ => _.Key).ToArray());
                    sql_where += !is_other ? "and j.type=@Type" :
                        !cfys.Any() ? "" :
                        $@"and (j.type is null or j.type not in ({string.Join(',', cfys.Select(_ => _.Key))}) ) ";
                }

                sql = string.Format(sql, sql_where);
                sql = $@"
select count(1) from ({sql}) T
;
select * from ({sql}) T
order by authentication desc,CreateTime desc 
OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
";
                var gr = await unitOfWork.QueryMultipleAsync(sql, new DynamicParameters(query));
                cc = await gr.ReadFirstAsync<int>();
                items = gr.Read<PcOrgItemDto, long, PcOrgItemDto>((x, no) =>
                {
                    x.Id_s = UrlShortIdUtil.Long2Base32(no);
                    return x;
                }, "no").AsArray();

                if (query.PageIndex == 1)
                {
                    await redis.SetAsync(rdk, new { totalItemCount = cc, page1_items = items }, 60 * 60);
                }
            }
            var pg = items.ToPagedList(query.PageSize, query.PageIndex, cc);

            // for CourceCount + EvaluationCount
            var dict = await mediator.Send(new PcGetOrgsCountsQuery { OrgIds = pg.CurrentPageItems.Select(_ => _.Id) });
            foreach (var item in pg.CurrentPageItems)
            {
                if (!dict.TryGetValue(item.Id, out var m)) continue;
                item.CourceCount = m.CourceCount;
                item.EvaluationCount = m.EvaluationCount;
            }

            return pg;
        }

        IEnumerable<int> GetMainSubj(bool includeOther = false)
        {
            foreach (var c1 in config.GetSection("AppSettings:pc_orgListPage_subjSide").GetChildren())
            {
                var temp = c1["item2"];
                if (!temp.IsNullOrWhiteSpace())
                {
                    var i = int.TryParse(temp, out var _i) ? _i : 0;
                    if (!includeOther && i.In(-1, 0, (int)OrgCfyEnum.Other)) continue;
                    yield return i;
                }
            }
        }
    }
}
