using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Activity.Appliaction.ResponseModels
{
#nullable enable

    public class ExcellentCasesEvltPageResult
    {
        /// <summary>优秀案例列表</summary>
        public IEnumerable<Organization.Appliaction.ResponseModels.EvaluationItemDto> Items { get; set; } = default!;
        /// <summary>海报</summary>
        public string? Banner { get; set; }
        /// <summary>活动(推广)码</summary>
        public string? Pcode { get; set; }
        /// <summary>活动数据,不为null</summary>
        public Organization.Appliaction.ResponseModels.ActivityDataDto ActivityData { get; set; } = default!;
    }

#nullable disable
}
