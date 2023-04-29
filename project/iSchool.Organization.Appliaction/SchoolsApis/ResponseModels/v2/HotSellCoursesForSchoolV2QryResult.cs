using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class HotSellCoursesForSchoolV2QryResult
    {
        public DateTime? Time { get; set; }

        /// <summary>热卖课程</summary>
        public IEnumerable<PcCourseItemDto2> HotSellCourses { get; set; } = default!;
    }

    public class HotSellOrgsForSchoolV2QryResult
    {
        public DateTime? Time { get; set; }

        /// <summary>推荐机构</summary>
        public IEnumerable<PcOrgItemDto> RecommendOrgs { get; set; } = default!;
    }
}

#nullable disable
