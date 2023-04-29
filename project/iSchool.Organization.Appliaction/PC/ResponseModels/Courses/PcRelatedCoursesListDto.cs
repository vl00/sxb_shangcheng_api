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
    /// <summary>相关课程</summary>   
    public class PcRelatedCoursesListDto
    {
        /// <summary>机构短id,用于跳转</summary>
        public string? OrgId { get; set; }
        /// <summary>
        /// 用于页面跳转的科目数值<br/>        
        /// 数值为null或0 科目栏选中全部
        /// </summary>
        public int? Subj { get; set; }

        /// <summary>相关课程</summary>        
        public IEnumerable<PcCourseItemDto> Courses { get; set; } = default!;
    }
}
#nullable disable
