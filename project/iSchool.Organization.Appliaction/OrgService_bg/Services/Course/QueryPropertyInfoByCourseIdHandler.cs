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
    /// 机构后台--获取课程属性-选项信息
    /// </summary>
    public class QueryPropertyInfoByCourseIdHandler : IRequestHandler<QueryPropertyInfoByCourseId, List<PropertyAndItems>>
    {
        
        OrgUnitOfWork _orgUnitOfWork;        

        public QueryPropertyInfoByCourseIdHandler(IOrgUnitOfWork unitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;            
        }

        public Task<List<PropertyAndItems>> Handle(QueryPropertyInfoByCourseId request, CancellationToken cancellationToken)
        {
            
            string sql = $@" 
select id as PropertyId,[Name] as PropertyName,4 as Operation,Sort 
, (
SELECT id as ItemId,[name] as ItemName,4 as Operation,sort FROM [dbo].[CoursePropertyItem]
where IsValid=1 and Propid=prop.Id order by Sort FOR JSON PATH
) as ProItemsJson
from [dbo].[CourseProperty] prop 
where IsValid=1 and Courseid='{request.CourseId}'
order by prop.Sort
;";
            var response = _orgUnitOfWork.DbConnection.Query<PropertyAndItems>(sql).ToList();
            for (int i = 0; i < response.Count; i++)
            {
                response[i].ProItems = JsonSerializationHelper.JSONToObject<List<ProItem>>(response[i].ProItemsJson);
            }
            return Task.FromResult(response);
        }

    }
}
