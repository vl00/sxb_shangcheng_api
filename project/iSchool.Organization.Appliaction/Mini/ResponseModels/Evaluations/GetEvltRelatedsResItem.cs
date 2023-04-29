using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class GetEvltRelatedsResItem
    {
        public Guid EvltId { get; set; } = default!;

        /// <summary>
        /// 关联主体mode 1=课程 2=品牌 3=其他
        /// </summary>
        public int? RelatedMode { get; set; }
        /// <summary>关联的课程.可null</summary>
        public IEnumerable<MiniEvltRelatedCourseDto>? RelatedCourses { get; set; }
        /// <summary>关联的品牌.可null</summary>
        public IEnumerable<MiniEvltRelatedOrgDto>? RelatedOrgs { get; set; }
    }

#nullable disable
}
