using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// PV content类型
    /// </summary>
    public enum PVisitCttTypeEnum
    {
        /// <summary>评测</summary>
        [Description("评测")]
        Evaluation = 1,
        /// <summary>课程</summary>
        [Description("课程")]
        Course = 2,
        /// <summary>机构</summary>
        [Description("机构")]
        Organization = 3,
    }
}
