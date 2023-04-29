using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 评测主页
    /// </summary>
    public class EvaluationIndexQueryResult
    {
        // banner //广告?

        /// <summary>
        /// 评测s
        /// </summary>
        public IEnumerable<EvaluationItemDto> Evaluations { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPageCount { get; set; }
        /// <summary>
        /// 科目(推荐=0)
        /// </summary>
        public IEnumerable<(string Name, string Id)> Subjs { get; set; }
        
    
    }

    public class EvaluationIndexQueryResult2
    {
        /// <summary>页面信息</summary>
        public LoadMoreResult<EvaluationItemDto> PageInfo { get; set; }
        /// <summary>科目(推荐=0)</summary>
        public IEnumerable<(string Name, string Id)> Subjs { get; set; }
    }
}
