using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class EvaluationEnumsResult
    {
        /// <summary>
        /// 科目<br/>
        /// item1: 用于传值<br/>
        /// item2: 用于显示<br/>
        /// </summary>
        public IEnumerable<(int, string)> Subject { get; set; } = default!;
        /// <summary>
        /// 年龄段<br/>
        /// item1: 用于传值<br/>
        /// item2: 用于显示<br/>
        /// </summary>
        public IEnumerable<(int, string)> AgeGroup { get; set; } = default!;
        /// <summary>
        /// 上课时长<br/>
        /// item1: 用于传值<br/>
        /// item2: 用于显示<br/>
        /// </summary>
        public IEnumerable<(int, string)> CourceDuration { get; set; } = default!;
        /// <summary>
        /// 教学模式/上课方式<br/>
        /// item1: 用于传值<br/>
        /// item2: 用于显示<br/>
        /// </summary>
        public IEnumerable<(int, string)> TeachMode { get; set; } = default!;
        /// <summary>
        /// 发评测时的专业模式维度step与问题内容提示<br/>
        /// item1: 问题<br/>
        /// item2: 随机的提示<br/>
        /// </summary>
        public IEnumerable<(string, string)> m2s { get; set; } = default!;
    }

#nullable disable
}
