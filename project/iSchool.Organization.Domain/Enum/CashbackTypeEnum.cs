using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 分销返还资金类型
    /// </summary>
    public enum CashbackTypeEnum
    {
        /// <summary>
        /// 百分比
        /// </summary>
        [Description("%")]
        Percent = 1,
        /// <summary>
        /// 元
        /// </summary>
        [Description("元")]
        Yuan = 2,
    }
}
