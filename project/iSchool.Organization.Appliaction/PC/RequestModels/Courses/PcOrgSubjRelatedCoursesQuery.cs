using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 机构|相同科目 相关课程s
    /// </summary>
    public class PcOrgSubjRelatedCoursesQuery : IRequest<PcOrgSubjRelatedCoursesQueryResult>
    {
        public int Len { get; set; } = 3;

        /// <summary>机构id</summary>
        public Guid OrgId { get; set; }
        /// <summary>课程id</summary>
        public Guid? CourseId { get; set; }
    }
}
#nullable disable
