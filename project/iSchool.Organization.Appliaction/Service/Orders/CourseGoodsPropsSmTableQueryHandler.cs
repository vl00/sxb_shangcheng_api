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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.Services
{
    public class CourseGoodsPropsSmTableQueryHandler : IRequestHandler<CourseGoodsPropsSmTableQuery, CourseGoodsPropsSmTableItemDto[]>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        IUserInfo me;
        CSRedisClient _redis;        
        IMapper _mapper;
        IConfiguration _config;

        public CourseGoodsPropsSmTableQueryHandler(IOrgUnitOfWork orgUnitOfWork, IMediator mediator, CSRedisClient redis, 
            IUserInfo me, IConfiguration config,
            IMapper mapper)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
            this._mediator = mediator;
            this.me = me;
            this._redis = redis;            
            this._mapper = mapper;
            this._config = config;
        }

        public async Task<CourseGoodsPropsSmTableItemDto[]> Handle(CourseGoodsPropsSmTableQuery query, CancellationToken cancellation)
        {
            var courseId = query.CourseId;
            var rdk = CacheKeys.CourseGoodsProps.FormatWith(courseId);
            var table = await _redis.GetAsync<CourseGoodsPropsSmTableItemDto[]>(rdk);
            if (table == null)
            {
                var sql = @"
select g.[id] as GoodsId,g.[CourseId],g.Price,g.Costprice,
p.[id] as PropGroupId,p.[name] as PropGroupName,p.[sort] as Sort_pg, 
i.[id] as PropItemId,i.[name] as PropItemName,i.[Cover] as PropItemCover,i.[sort] as Sort_i
,cge.Id CourseGoodsExchangeId,cge.Point Points,cge.Price
from CourseGoods g
left join CourseGoodsPropItem gi on g.id=gi.goodsid
left join CoursePropertyItem i on i.id=gi.PropItemId
left join CourseProperty p on p.id=i.Propid
left join CourseGoodsExchange cge on cge.GoodId = g.id and  cge.IsValid =1 AND cge.Show=1
where g.IsValid=1 and g.show=1 and i.IsValid=1 and p.IsValid=1 
and g.Courseid=@courseId
order by p.sort,i.sort
";
                table = (await _orgUnitOfWork.QueryAsync<
                        (Guid GoodsId, Guid CourseId, decimal Price, decimal Costprice)
                        , (Guid PropGroupId, string PropGroupName, int Sort_pg)
                        , (Guid PropItemId, string PropItemName, string PropItemCover,int Sort_i)
                        , PointsExchangeInfo
                        , (
                            (Guid GoodsId, Guid CourseId, decimal Price, decimal Costprice)
                            , (Guid PropGroupId, string PropGroupName, int Sort_pg)
                            , (Guid PropItemId, string PropItemName,string PropItemCover, int Sort_i)
                            , PointsExchangeInfo)>
                    (sql,
                    splitOn: "PropGroupId,PropItemId,CourseGoodsExchangeId",
                    param: new { courseId },
                    map: (c, pg, pi,pointsExchangeInfo) =>
                    {
                        return (c, pg, pi, pointsExchangeInfo);
                    }
                )).GroupBy(x => x.Item1).Select(g => 
                {
                    var r = new CourseGoodsPropsSmTableItemDto();
                    r.GoodsId = g.Key.GoodsId;
                    r.CourseId = g.Key.CourseId;
                    r.Price = g.Key.Price;
                    r.Costprice = g.Key.Costprice;
                    r.PointExchange = g.First().Item4;
                    r.PropItems = g.Select(x => new CourseGoodsPropsSmTableItemDto_PropItem 
                    {
                        PropGroupId = x.Item2.PropGroupId,
                        PropGroupName = x.Item2.PropGroupName,
                        Sort_pg = x.Item2.Sort_pg,
                        PropItemId = x.Item3.PropItemId,
                        PropItemName = x.Item3.PropItemName,
                        //PropItemCover=x.Item3.PropItemCover,
                        Sort_i = x.Item3.Sort_i,
                    }).ToArray();
                    return r;
                }).ToArray();

                await _redis.SetAsync(rdk, table.ToJsonString(camelCase: true), 60 * 60 * 24 * 1);
            }

            if (table != null)
            {
                foreach (var item in table)
                {
                    item.Stock = (await _mediator.Send(new CourseGoodsStockRequest
                    {
                        GetStock = new GetGoodsStockQuery { Id = item.GoodsId, FromDBIfNotExists = true }
                    })).GetStockResult ?? 0;
                }
            }

            return table;
        }

    }
}
