using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 上课时长
    /// </summary>
    public enum CourceDurationEnum
    {
        /// <summary>
        /// <30分钟
        /// </summary>
        [Description("<30分钟")]
        Dura1 = 1,
        /// <summary>
        /// 30-60分钟
        /// </summary>
        [Description("30-60分钟")]
        Dura2 = 2,
        /// <summary>
        /// 1小时以上
        /// </summary>
        [Description("1小时以上")]
        Dura3 = 3,
    }
}
