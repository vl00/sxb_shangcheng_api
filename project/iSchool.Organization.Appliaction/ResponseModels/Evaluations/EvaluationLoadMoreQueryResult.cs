using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 评测主页下的评测列表result.第一页会返回科目
    /// </summary>
    public class EvaluationLoadMoreQueryResult : LoadMoreResult<EvaluationItemDto>
    {
        /// <summary>
        /// 科目(推荐=0), 第一页会返回科目
        /// </summary>
        public IEnumerable<(string Name, string Id)> Subjs { get; set; }
    }
}
