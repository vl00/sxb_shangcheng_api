using Dapper;
using iSchool.Infrastructure;
using System;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
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
    public class QueryBigCourseInfoByCourseIdHandler : IRequestHandler<QueryBigCourseInfoByCourseId, List<BigCourse>>
    {
        
        OrgUnitOfWork _orgUnitOfWork;        

        public QueryBigCourseInfoByCourseIdHandler(IOrgUnitOfWork unitOfWork)
        {
            this._orgUnitOfWork = (OrgUnitOfWork)unitOfWork;            
        }

        public Task<List<BigCourse>> Handle(QueryBigCourseInfoByCourseId request, CancellationToken cancellationToken)
        {
            
            string sql = $@" 
select * from  [dbo].[BigCourse]  where IsValid=1 and Courseid='{request.CourseId}'
order by CreateTime
;";
            var response = new List<BigCourse>();
             response = _orgUnitOfWork.DbConnection.Query<BigCourse>(sql).ToList();
            if (response?.Any() == false)
            {
                response.AddRange(new List<BigCourse>() { 
                     new BigCourse() { Id=Guid.NewGuid(), CashbackType= (byte)CashbackTypeEnum.Percent, HeadFxUserExclusiveType=(byte)CashbackTypeEnum.Percent, Price=0 } 
                    ,new BigCourse() {Id=Guid.NewGuid(), CashbackType= (byte)CashbackTypeEnum.Percent, HeadFxUserExclusiveType=(byte)CashbackTypeEnum.Percent, Price=0 } });
            }
            else if (response.Count == 1)
            {
                response.AddRange(new List<BigCourse>() {
                     new BigCourse() {Id=Guid.NewGuid(), CashbackType= (byte)CashbackTypeEnum.Percent, HeadFxUserExclusiveType=(byte)CashbackTypeEnum.Percent, Price=0 }
                    });
            }
            return Task.FromResult(response);
        }

    }
}
