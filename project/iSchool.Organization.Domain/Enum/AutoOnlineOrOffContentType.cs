using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 自动上下线内容type
    /// </summary>
    public enum AutoOnlineOrOffContentType
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
    }
}
