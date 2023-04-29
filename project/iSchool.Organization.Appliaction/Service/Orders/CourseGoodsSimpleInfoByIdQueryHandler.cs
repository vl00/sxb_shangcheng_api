using AutoMapper;
using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Infras;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Extensions;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Domain.Security;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
    public class CourseGoodsSimpleInfoByIdQueryHandler : IRequestHandler<CourseGoodsSimpleInfoByIdQuery, CourseGoodsSimpleInfoDto>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;
        CSRedisClient _redis;
        IConfiguration _config;

        public CourseGoodsSimpleInfoByIdQueryHandler(IOrgUnitOfWork unitOfWork, IMediator mediator, CSRedisClient redis,
            IConfiguration config)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = mediator;
            this._redis = redis;
            this._config = config;
        }

        public async Task<CourseGoodsSimpleInfoDto> Handle(CourseGoodsSimpleInfoByIdQuery query, CancellationToken cancellation)
        {            
            CourseGoodsSimpleInfoDto result = default!; 
            DateTime courseModifyDateTime = default;
            Course course = default!;
            await default(ValueTask);

            //
            // try find in cache            
            if (query.UseCache)
            {
                var r = await _redis.GetAsync<(DateTime, CourseGoodsSimpleInfoDto)>(CacheKeys.CourseGoodsInfo.FormatWith(query.GoodsId));
                if (r != default)
                {
                    courseModifyDateTime = r.Item1;
                    result = r.Item2;
                }
            }
            //
            // not in cache or cache is old
            LB_find_in_db:
            if (result == null)
            {
                var sql = $@"
select g.Id,g.CourseId,g.Price,g.Origprice,g.LimitedBuyNum as SkuLimitedBuyNum,g.cover,g.SupplierId,g.Costprice,g.ArticleNo,
i.id as i_id,i.name as i_name,(case when g.IsValid=1 and g.show=1 and i.IsValid=1 and p.IsValid=1 then 1 else 0 end)as IsValid
,cge.id cgeid,cge.point points,cge.price
from CourseGoods g
left join CourseGoodsPropItem gi on g.id=gi.goodsid
left join CoursePropertyItem i on i.id=gi.PropItemId
left join CourseProperty p on p.id=i.Propid
left join CourseGoodsExchange cge on cge.GoodId = g.id and  cge.IsValid =1 AND  cge.Show=1
where 1=1
and g.id=@GoodsId
order by p.sort,i.sort
";
                var ls = await _orgUnitOfWork.DbConnection.QueryAsync<CourseGoodsSimpleInfoDto, (Guid, string, bool), PointsExchangeInfo, CourseGoodsSimpleInfoDto >(sql,
                    splitOn: "i_id,cgeid",
                    param: new { query.GoodsId },
                    map: (dto, i, pointsExchangeInfo) =>
                    {                        
                        dto.PropItems = new[] { new CoursePropItemsListItemDto { Id = i.Item1, Name = i.Item2 } };
                        dto.IsValid = i.Item3;
                        dto.PointExchange = pointsExchangeInfo;
                        return dto;
                    }
                );
                result = ls.FirstOrDefault() ?? new CourseGoodsSimpleInfoDto { IsValid = false };
                result.PropItems = ls.SelectMany(x => x.PropItems).ToArray();
            }
            //
            // try check cache
            if (result != null)
            {
                course = await _mediator.Send(new CourseBaseInfoQuery { CourseId = result.CourseId, AllowNotValid = true });
                if (courseModifyDateTime == (course.ModifyDateTime ?? course.CreateTime))
                {
                    // cache is ok
                    goto LB_next;
                }
                else if (courseModifyDateTime != default)
                {
                    // cache is old, need reset from db
                    courseModifyDateTime = default;
                    result = null;
                    goto LB_find_in_db;
                }
                // no cache but find from db
                courseModifyDateTime = course.ModifyDateTime ?? course.CreateTime! ?? default;                

                // set or reset to cache
                await _redis.SetAsync(CacheKeys.CourseGoodsInfo.FormatWith(query.GoodsId), (courseModifyDateTime, result), 60 * 15);
            }

            LB_next:
            if (result != null && result.Id != default)
            {
                result.Cover ??= course.Videocovers?.ToObject<string[]>()?.ElementAtOrDefault(0) ?? course.Banner?.ToObject<string[]>()?.ElementAtOrDefault(0);
                result.Type = course.Type;
                result._Course = query.NeedCourse ? course : null;
                result.SupplierId ??= default;
                if (result.IsValid) result.IsValid = course.IsValid && course.Status == CourseStatusEnum.Ok.ToInt();

                // null为不限购
                var skuLimitedBuyNum = result.SkuLimitedBuyNum ?? -1;
                var spuLimitedBuyNum = course.LimitedBuyNum ?? -1;
                result.SkuLimitedBuyNum = skuLimitedBuyNum > 0 ? skuLimitedBuyNum : (int?)null;
                result.SpuLimitedBuyNum = spuLimitedBuyNum > 0 ? spuLimitedBuyNum : (int?)null;
                //
                result.LimitedBuyNumForThisTurn = skuLimitedBuyNum > 0 && spuLimitedBuyNum > 0 ? Math.Min(skuLimitedBuyNum, spuLimitedBuyNum)
                    : result.SkuLimitedBuyNum ?? result.SpuLimitedBuyNum;

                // 库存
                if (result.IsValid)
                {
                    result.Stock = (await _mediator.Send(new CourseGoodsStockRequest
                    {
                        GetStock = new GetGoodsStockQuery { Id = result.Id, FromDBIfNotExists = true }
                    })).GetStockResult ?? 0;
                }
            }

            return result.Id == default ? null : result;
        }

    }
}
