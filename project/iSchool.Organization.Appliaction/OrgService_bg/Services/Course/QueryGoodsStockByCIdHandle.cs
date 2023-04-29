using CSRedis;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iSchool.Organization.Domain.Enum;
using iSchool.Organization.Appliaction.ViewModels.Courses;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{

    /// <summary>
    /// 机构后台--获取课程详情
    /// </summary>
    public class QueryGoodsStockByCIdHandle : IRequestHandler<QueryGoodsStockByCId, List<GoodsStockInfo>>
    {
        
        OrgUnitOfWork _orgUnitOfWork;        

        public QueryGoodsStockByCIdHandle(IOrgUnitOfWork unitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;            
        }

        public async Task<List<GoodsStockInfo>> Handle(QueryGoodsStockByCId request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;           
            List<GoodsStockInfo> response = new List<GoodsStockInfo>();

            var goods= _orgUnitOfWork.DbConnection.Query<CourseGoods>($"select * from CourseGoods where IsValid=1 and Courseid='{request.CourseId}'").ToList();

            var items = _orgUnitOfWork.DbConnection.Query<CGoodsProItemInfo>(
                @$"
select pro.name+'-'+item.Name AS itemname,gitem.GoodsId from [dbo].[CourseProperty] as pro 
left join [dbo].[CoursePropertyItem] as item on pro.Id=item.Propid and item.IsValid=1
left join [dbo].[CourseGoodsPropItem] as gitem on gitem.PropItemId=item.Id 
where pro.IsValid=1 and  pro.Courseid='{request.CourseId}'
order by pro.Sort,item.Sort
"
                ).ToList();
            foreach (var g in goods)
            {
                response.Add(new GoodsStockInfo() {
                     Goods=g,
                     Items= items.Where(_=>_.GoodsId==g.Id).ToList()
                });
            }
            return response;
        }

    }
}
