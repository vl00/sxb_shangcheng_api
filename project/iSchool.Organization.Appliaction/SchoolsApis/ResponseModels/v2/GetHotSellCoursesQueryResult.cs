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
    /// v2热卖课程
    /// </summary>
    public class GetHotSellCoursesQueryResult
    {
        /// <summary>热卖课程</summary>
        public PagedList<PcCourseItemDto2> PageInfo { get; set; } = default!;

        ///// <summary>课程列表科目栏目</summary>
        //public IEnumerable<SelectItemsKeyValues>? Subjs { get; set; }
    }    

#nullable disable
}
