using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Organization.Appliaction.RequestModels.Courses;
using MediatR;
using iSchool.Organization.Domain;
using System.Threading.Tasks;
using System.Threading;
using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using System.Linq;

namespace iSchool.Organization.Appliaction.OrgService_bg
{
    /// <summary>
    /// 根据科目Id，获取课程列表
    /// </summary>
    public class CoursesBySubjectIdQueryHandler : IRequestHandler<CoursesBySubjectIdQuery, List<CourseSelectItem>>
    {
        OrgUnitOfWork _orgUnitOfWork;
        public CoursesBySubjectIdQueryHandler(IOrgUnitOfWork orgUnitOfWork)
        {
            _orgUnitOfWork = (OrgUnitOfWork)orgUnitOfWork;
        }
        public Task<List<CourseSelectItem>> Handle(CoursesBySubjectIdQuery request, CancellationToken cancellationToken)
        {
            if (Enum.IsDefined(typeof(SubjectEnum), request.SubjectId))
            {
                string sql = $@" select id,title from dbo.Course where IsValid=1 and [subject]={request.SubjectId} and type={CourseTypeEnum.Course.ToInt()};";
                var response = _orgUnitOfWork.Query<CourseSelectItem>(sql).ToList();
                return Task.FromResult(response);
            }
            
            throw new NotImplementedException();
        }
    }

    public class CourseSelectItem 
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }

}
