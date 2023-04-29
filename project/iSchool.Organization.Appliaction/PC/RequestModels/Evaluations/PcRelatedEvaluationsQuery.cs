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
    /// pc评测详情|课程详情 相关评测s
    /// </summary>
    public class PcRelatedEvaluationsQuery : IRequest<PcRelatedEvaluationsListDto>
    {
        /// <summary>查询数目</summary>
        public int Len { get; set; }

        /// <summary>评测id</summary>
        public Guid? EvltId { get; set; }
        /// <summary>课程id</summary>
        public Guid? CourseId { get; set; }
        /// <summary>科目</summary>
        public int? Subj { get; set; }

        /// <summary>机构id</summary>
        public Guid? OrgId { get; set; } 
    }
}
#nullable disable
