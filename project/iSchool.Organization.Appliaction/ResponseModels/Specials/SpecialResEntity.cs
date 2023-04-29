using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 单个专题页
    /// </summary>
    public class SpecialResEntity
    {
        /// <summary>
        /// 专题id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 副标题
        /// </summary>
        public string SubTitle { get; set; }
        /// <summary>
        /// 专题的图片/海报
        /// </summary>
        public string Banner { get; set; }
        /// <summary>
        /// 评测列表项s
        /// </summary>
        public IEnumerable<EvaluationItemDto> Evaluations { get; set; } = new EvaluationItemDto[0];
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPageCount { get; set; }
        /// <summary>
		/// 专题分享标题
		/// </summary>
        public string ShareTitle { get; set; }
        /// <summary>
        /// 专题分享副标题
        /// </summary>
        public string ShareSubTitle { get; set; }
    }
}
