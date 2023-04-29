using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Domain;
using MediatR;
using System.Linq;

namespace iSchool.Organization.Appliaction.OrgService_bg.Course
{
    /// <summary>
    /// 课程购前须知集合
    /// </summary>
    public class QueryCourseNoticesByCIdHandler : IRequestHandler<QueryCourseNoticesByCId, List<CourseNotices>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public QueryCourseNoticesByCIdHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }
        public Task<List<CourseNotices>> Handle(QueryCourseNoticesByCId request, CancellationToken cancellationToken)
        {
            string sql = $@"  select * from [dbo].[CourseNotices] where IsValid=1 and CourseId='{request.CourseId}' ;";
            var response= _orgUnitOfWork.Query<CourseNotices>(sql).ToList();
            return Task.FromResult(response);
        }
    }
}
