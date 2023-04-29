using Dapper;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.Service.EvaluationCrawler;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Appliaction.ViewModels.Special;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 后台管理--商品列表
    /// </summary>
    public class SearchGoodsInfoQueryHandler : IRequestHandler<SearchGoodsInfoQuery, CourseGoodsInfo>
    {
        OrgUnitOfWork _orgUnitOfWork;
        IMediator _mediator;

        public SearchGoodsInfoQueryHandler(IOrgUnitOfWork unitOfWork, IMediator _mediator)
        {
            _orgUnitOfWork = (OrgUnitOfWork)unitOfWork;
            this._mediator = _mediator;
        }

        public async Task<CourseGoodsInfo> Handle(SearchGoodsInfoQuery request, CancellationToken cancellationToken)
        {
            var response = new CourseGoodsInfo();
            await Task.CompletedTask;
            string goodsListSql = $@" 
 select goods.Courseid,goods.Id as GoodsId,goods.SupplierId,goods.Stock,goods.Price,goods.Origprice,goods.LimitedBuyNum,goods.Show,goods.Cover,goods.Costprice,goods.ArticleNo,goods.SupplieAddressId
from  [dbo].[CourseGoods] as goods 
left join [dbo].[CourseGoodsPropItem] gi on goods.Id=gi.goodsid    
left join [dbo].[CoursePropertyItem] i on i.id=gi.propitemid
left join [dbo].[CourseProperty] p on p.id=i.propid
where goods.IsValid=1 and i.isvalid=1 and p.isvalid=1
and goods.Courseid='{request.CourseId}'
order by p.sort,i.sort
                            ;";
            string ProItemListSql = $@" 
select cp.GoodsId,(pro.Name+'-'+ item.Name )as PropertyItemName,item.Sort from [dbo].[CourseProperty] pro
left join [dbo].[CoursePropertyItem] item on item.Propid=pro.Id and item.IsValid=1
left join [dbo].[CourseGoodsPropItem] cp on item.Id=cp.PropItemId   
where item.IsValid=1 and item.Courseid='{request.CourseId}'
order by cp.GoodsId,pro.Sort,item.Sort
                             ;";
            response.CourseGoods = _orgUnitOfWork.DbConnection.Query<GoodsInfo>(goodsListSql).ToList();
            var ProItemList= _orgUnitOfWork.DbConnection.Query<PropertyItemInfo>(ProItemListSql);
            var proCount = 0;
            for (int i = 0; i < response.CourseGoods.Count; i++)
            {
                var goods = response.CourseGoods[i];   
                var items= ProItemList.Where(_ => _.GoodsId == goods.GoodsId).ToList();
                goods.PropertyItemNames = items;
                proCount = items?.Count ?? 0;
            }
            response.PropertyCount = proCount;

            // 供应商的地址s
            foreach (var item in response.CourseGoods)
            {
                try
                {
                    item.SupplieAddresses = (await _mediator.Send(new SelectItemsQuery { Type = 10, SupplierId = item.SupplierId })).AsArray();
                }
                catch { }
            }

            // sku(直推)佣金
            await GetSkuCashback(response, request.CourseId);
            
            // sku积分
            await GetSkuPointCashback(response, request.CourseId);

            // sku兑换积分
            await GetSkuPointExchanges(response, request.CourseId);

            return response;
        }

        // sku(直推)佣金
        async Task GetSkuCashback(CourseGoodsInfo response, Guid courseId)
        {
            var sql = $"select * from CourseGoodDrpInfo d where d.IsValid=1 and d.courseId=@courseId";
            var ls = await _orgUnitOfWork.QueryAsync<CourseGoodDrpInfo>(sql, new { courseId });

            foreach (var item in response.CourseGoods)
            {
                if (!ls.TryGetOne(out var g, _ => _.GoodId == item.GoodsId)) continue;
                item.CashbackValue = g.CashbackValue;
                item.CashbackType = g.CashbackType;
            }
        }

        // sku积分
        async Task GetSkuPointCashback(CourseGoodsInfo response, Guid courseId)
        {
            var sql = $"select * from CourseGoodPointCashBack d where d.IsValid=1 and d.courseId=@courseId";
            var ls = await _orgUnitOfWork.QueryAsync<CourseGoodPointCashBack>(sql, new { courseId });

            foreach (var item in response.CourseGoods)
            {
                if (!ls.TryGetOne(out var g, _ => _.GoodId == item.GoodsId)) continue;
                item.PointCashBackValue = g.PointCashBackValue;
                item.PointCashBackType = g.PointCashBackType;
            }
        }

        // sku兑换积分
        async Task GetSkuPointExchanges(CourseGoodsInfo response, Guid courseId)
        {
            var sql = "select IsPointExchange from Course c where c.id=@courseId";
            var b = await _orgUnitOfWork.QueryFirstOrDefaultAsync<bool?>(sql, new { courseId });
            response.EnablePointExchange = b ?? false;

            sql = $"select * from CourseGoodsExchange e where e.IsValid=1 and e.courseId=@courseId";
            var ls = await _orgUnitOfWork.QueryAsync<CourseGoodsExchange>(sql, new { courseId });

            response.SkuPointExchanges = new List<SkuPointExchangeItem>();
            foreach (var item in response.CourseGoods)
            {
                if (ls.TryGetOne(out var g, _ => _.GoodId == item.GoodsId))
                {
                    response.SkuPointExchanges.Add(new SkuPointExchangeItem
                    {
                        CourseId = courseId,
                        GoodsId = item.GoodsId,
                        PropertyItemNames = item.PropertyItemNames,
                        Point = g.Point,
                        Price = g.Price,
                        Show = g.Show,
                    });
                }
                else
                {
                    response.SkuPointExchanges.Add(new SkuPointExchangeItem
                    {
                        CourseId = courseId,
                        GoodsId = item.GoodsId,
                        PropertyItemNames = item.PropertyItemNames,
                    });
                }
            }
        }

    }
}
