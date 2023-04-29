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
    public class GetRecommendOrgsQueryHandler : IRequestHandler<GetRecommendOrgsQuery, GetRecommendOrgsQueryResult>
    {
        OrgUnitOfWork _unitOfWork;
        IMediator _mediator;
        IUserInfo me;
        CSRedisClient redis;        
        IMapper _mapper;
        IConfiguration _config;

        public GetRecommendOrgsQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis, 
            IUserInfo me, IConfiguration config,
            IMapper mapper)
        {
            this._unitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this.me = me;
            this.redis = redis;            
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<GetRecommendOrgsQueryResult> Handle(GetRecommendOrgsQuery query, CancellationToken cancellation)
        {
            var result = new GetRecommendOrgsQueryResult();

            var rr = await _mediator.Send(new PcGetSubjRecommendOrgsQuery 
            {
                PageIndex = query.PageIndex,
                PageSize = query.PageSize,
                Type = query.Type,
            });

            result.AllOrgTypes = rr.AllOrgTypes;

            result.PageInfo = rr.PageInfo;
            // urls
            {
                foreach (var item in result.PageInfo.CurrentPageItems)
                {
                    item.MUrl = $"{_config["BaseUrls:org-m"]}/orgs/detail/{item.Id_s}";
                    item.PcUrl = $"{_config["BaseUrls:org-pc"]}/orgs/detail/{item.Id_s}";

                    // mpqrcode
                    try
                    {
                        var cmd1 = _config.GetSection("AppSettings:CreateMpQrcode:org-detail").Get<CreateMpQrcodeCmd>();
                        cmd1.Scene = cmd1.Scene.FormatWith(item.Id_s);
                        item.MpQrcode = (await _mediator.Send(cmd1)).MpQrcode;
                    }
                    catch { /* ignore */ }
                }
            }

            return result;
        }

//        async Task<PagedList<PcOrgItemDto>> GetPagedList(GetRecommendOrgsQuery query)
//        {
//            PcOrgItemDto[] items = null;
//            int cc = 0;
//            await default(ValueTask);

//            // redis key
//            var rdk = CacheKeys.ToSchoolsv2_RecommendOrgsLs.FormatWith(query.PageSize, query.Type);

//            if (query.PageIndex == 1)
//            {
//                var jtk = await redis.GetAsync<JToken>(rdk);
//                if (jtk != null)
//                {
//                    cc = (int)jtk["totalItemCount"];
//                    items = jtk["page1_items"].ToObject<PcOrgItemDto[]>();
//                }
//            }
//            if (items == null) // find in db sql
//            {
//                var sql = $@"
//select count(1) from (
//select distinct o.id,o.name,o.logo,o.authentication,o.[desc],o.subdesc,o.types,o.subjects,o.CreateTime,isnull(pv.viewcount,0)as viewcount,o.no
//from Organization o
//left join Pv4Org pv on pv.IsValid=1 and pv.orgid=o.id 
//outer apply openjson(o.types) with([type] int '$') j
//where o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} and o.[authentication]=1
//{{0}}
//)T
//---
//select * from(
//select distinct o.id,o.name,o.logo,o.authentication,o.[desc],o.subdesc,o.types,o.subjects,o.CreateTime,isnull(pv.viewcount,0)as viewcount,o.no
//from Organization o
//left join Pv4Org pv on pv.IsValid=1 and pv.orgid=o.id 
//outer apply openjson(o.types) with([type] int '$') j
//where o.IsValid=1 and o.status={OrganizationStatusEnum.Ok.ToInt()} and o.[authentication]=1
//{{0}}
//)T order by viewcount desc,CreateTime desc 
//OFFSET (@PageIndex-1)*@PageSize ROWS FETCH NEXT @PageSize ROWS ONLY
//";
//                var sql_where = new StringBuilder();
//                if (query.Type > 0)
//                {
//                    var is_other = query.Type.In(-1, OrgCfyEnum.Other.ToInt());
//                    sql_where.AppendLine(!is_other ? "and j.type=@Type" :                        
//                        $@"and (j.type is null or j.type not in (-1, {OrgCfyEnum.Other.ToInt()}) ) ");
//                }

//                sql = string.Format(sql, sql_where);

//                var gr = await _unitOfWork.QueryMultipleAsync(sql, new DynamicParameters(query));
//                cc = await gr.ReadFirstAsync<int>();
//                items = gr.Read<PcOrgItemDto, long, PcOrgItemDto>((x, no) =>
//                {
//                    x.Id_s = UrlShortIdUtil.Long2Base32(no);
//                    return x;
//                }, "no").AsArray();

//                if (query.PageIndex == 1)
//                {
//                    await redis.SetAsync(rdk, new { totalItemCount = cc, page1_items = items }, 60 * 60);
//                }
//            }
//            var pg = items.ToPagedList(query.PageSize, query.PageIndex, cc);

//            // for CourceCount + EvaluationCount
//            var dict = await _mediator.Send(new PcGetOrgsCountsQuery { OrgIds = pg.CurrentPageItems.Select(_ => _.Id) });
//            foreach (var item in pg.CurrentPageItems)
//            {
//                if (!dict.TryGetValue(item.Id, out var m)) continue;
//                item.CourceCount = m.CourceCount;
//                item.EvaluationCount = m.EvaluationCount;
//            }

            

//            return pg;
//        }

        IEnumerable<int> GetMainSubj(bool includeOther = false)
        {
            foreach (var c1 in _config.GetSection("AppSettings:pc_orgListPage_subjSide").GetChildren())
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
