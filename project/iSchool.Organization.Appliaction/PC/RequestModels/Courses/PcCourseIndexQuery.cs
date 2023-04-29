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
    /// 课程列表
    /// </summary>
    public class PcCourseIndexQuery : IRequest<PcCourseIndexQueryResult>
    {
        /// <summary>机构no</summary>
        public long? OrgNo { get; set; }
        /// <summary>科目</summary>
        public int? Subj { get; set; }
        /// <summary>品牌认证(展示所有认证的品牌)</summary>
        public bool? Authentication { get; set; }
        /// <summary>页码</summary>
        public int PageIndex { get; set; } = 1;
        /// <summary>页大小</summary>
        public int PageSize { get; set; }
    }
}
#nullable disable
