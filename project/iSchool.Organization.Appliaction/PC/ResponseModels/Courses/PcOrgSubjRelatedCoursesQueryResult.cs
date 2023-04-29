using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class PcOrgSubjRelatedCoursesQueryResult
    {
        public IEnumerable<PcCourseItemDto> Items { get; set; } = default!;
        /// <summary>科目</summary>
        public int Subj { get; set; }
        /// <summary>是否当前机构下的课程</summary>
        public bool IsCurrOrgCourses { get; set; }
    }
}
#nullable disable
