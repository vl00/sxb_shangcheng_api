using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Domain;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{

    /// <summary>
    /// 获取 商品-选项Id集合的列表
    /// </summary>
    public class QueryGoodsItemIdsByCourseIdHandler : IRequestHandler<QueryGoodsItemIdsByCourseId, List<GoodsItemIds>>
    {
        
        OrgUnitOfWork _orgUnitOfWork;        

        public QueryGoodsItemIdsByCourseIdHandler(IOrgUnitOfWork unitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;            
        }

        public Task<List<GoodsItemIds>> Handle(QueryGoodsItemIdsByCourseId request, CancellationToken cancellationToken)
        {
            
            string sql = $@" 
select goods.Id as GoodsId,4 as Operation 
, (
SELECT id as ItemId FROM [dbo].[CourseGoodsPropItem]
where IsValid=1 and GoodsId=goods.Id  FOR JSON PATH
) as ItemIdsJson
from [dbo].[CourseGoods] goods
where IsValid=1 and Courseid='{request.CourseId}'
;";
            var response = _orgUnitOfWork.DbConnection.Query<GoodsItemIds>(sql).ToList();
            for (int i = 0; i < response.Count; i++)
            {
                response[i].ItemIds = JsonSerializationHelper.JSONToObject<List<ItemIdModel>>(response[i].ItemIdsJson);
            }
            return Task.FromResult(response);
        }

    }
}
