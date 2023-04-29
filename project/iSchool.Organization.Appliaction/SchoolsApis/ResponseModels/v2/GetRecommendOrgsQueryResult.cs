using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// v2推荐机构
    /// </summary>
    public class GetRecommendOrgsQueryResult
    {
        /// <summary>推荐机构</summary>
        public PagedList<PcOrgItemDto> PageInfo { get; set; } = default!;

        /// <summary>机构分类科目栏目</summary>
        public IEnumerable<SelectItemsKeyValues>? AllOrgTypes { get; set; }
    }

#nullable disable
}
