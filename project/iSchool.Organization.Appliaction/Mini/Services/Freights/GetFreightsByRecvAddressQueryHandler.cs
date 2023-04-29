using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
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

namespace iSchool.Organization.Appliaction.Services
{
    public class GetFreightsByRecvAddressQueryHandler : IRequestHandler<GetFreightsByRecvAddressQuery, GetFreightsByRecvAddressQryResult>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public GetFreightsByRecvAddressQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<GetFreightsByRecvAddressQryResult> Handle(GetFreightsByRecvAddressQuery query, CancellationToken cancellation)
        {
            var result = new GetFreightsByRecvAddressQryResult();
            await default(ValueTask);

            if (query.Province.IsNullOrEmpty() || query.Province.Length < 2)
            {
                throw new CustomResponseException("省参数错误");
            }
            if ((query.SkuIds?.Length ?? -1) < 1)
            {
                throw new CustomResponseException("请选择商品");
            }
            query.SkuIds = query.SkuIds.Distinct().ToArray();

            var pr = await _mediator.Send(new GetFreightCityAreasTypeQyArgs { Province = query.Province });

            // 是否是在不发货(黑名单)里
            {
                var sql = $@"
select g.id,g.courseid,b.pr from CourseGoods g left join Course c on g.courseid=c.id
outer apply openjson(c.BlackList) with (pr int '$') b
where isjson(c.BlackList)=1 and (b.pr=@pr or b.pr=0) and g.id in @SkuIds
";
                var ls = (await _orgUnitOfWork.QueryAsync<(Guid SkuId, Guid CourseId, int Pr)>(sql, new { query.SkuIds, pr = pr.Code })).AsList();
                if (ls.Count > 0)
                {
                    result.BlacklistSkuIds = ls.Select(_ => _.SkuId).ToArray();
                    //return result;
                }
            }

            // 正常情况
            {
                var sql = $@"
select g.SupplierId,c.orgid,f.id as cfid,f.courseid,f.cost as Freight,f.type,f.[Name],f.Citys,j.* 
from CourseFreight f join Course c on c.id=f.CourseId
join CourseGoods g on g.courseid=c.id
outer apply openjson(f.Name) with(area nvarchar(20) '$') j
where f.IsValid=1 and g.id in @SkuIds
and (f.type=@type or (f.type={FreightAreaTypeEnum.Custom.ToInt()} and (j.area='全国' or left(j.area,2)=left(@p,2))) )
";
                var ls = await _orgUnitOfWork.QueryAsync<OrgFreightDto>(sql, new { query.SkuIds, type = pr.Ty.ToInt(), p = pr.Name });

                var freights = ls.GroupBy(_ => _.SupplierId).Select(g =>
                {
                    var maxFreight = g.Max(_ => _.Freight);
                    var o = g.FirstOrDefault(_ => _.Freight == maxFreight);
                    return o;
                }).Where(_ => _ != null).AsList();

                if (query.AllowFillEmptyOrgsFreights)
                {
                    sql = $"select isnull(SupplierId,'{Guid.Empty}') as SupplierId from CourseGoods where id in @SkuIds and isnull(SupplierId,'{Guid.Empty}') not in @ids";
                    var ids = await _orgUnitOfWork.QueryAsync<Guid>(sql, new { query.SkuIds, ids = ls.GroupBy(_ => _.SupplierId).Select(_ => _.Key) });
                    foreach (var id in ids)
                    {
                        freights.Add(new OrgFreightDto { SupplierId = id, Freight = 0 });
                    }
                }

                result.Freights = freights;
            }

            return result;
        }

    }
}
