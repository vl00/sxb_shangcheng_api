using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 活动扩展类型
    /// </summary>
    public enum ActivityExtendType
    {
        /// <summary>评测</summary>
        [Description("评测")]
        Evaluation = 1,
        /// <summary>机构</summary>
        [Description("机构")]
        Organization = 2,
        /// <summary>课程</summary>
        [Description("课程")]
        Course = 3,
        /// <summary>专题</summary>
        [Description("专题")]
        Special = 4,
    }
}
