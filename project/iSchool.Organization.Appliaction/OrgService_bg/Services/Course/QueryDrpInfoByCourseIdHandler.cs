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
    public class QueryDrpInfoByCourseIdHandler : IRequestHandler<QueryDrpInfoByCourseId, CourseDrpInfo>
    {
        
        OrgUnitOfWork _orgUnitOfWork;        

        public QueryDrpInfoByCourseIdHandler(IOrgUnitOfWork unitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;            
        }

        public Task<CourseDrpInfo> Handle(QueryDrpInfoByCourseId request, CancellationToken cancellationToken)
        {
            
            string sql = $@" 
select * from [dbo].[CourseDrpInfo] where IsValid=1 and Courseid='{request.CourseId}'
;";
            var response = _orgUnitOfWork.DbConnection.Query<CourseDrpInfo>(sql).FirstOrDefault();
            
            return Task.FromResult(response);
        }

    }
}
