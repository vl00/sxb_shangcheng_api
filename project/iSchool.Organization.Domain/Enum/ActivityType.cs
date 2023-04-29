using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 活动类型
    /// </summary>
    public enum ActivityType
    {
        /// <summary>未定义</summary>
        [Description("未定义")]
        None = 0,
        /// <summary>旧活动</summary>
        [Description("旧活动")]
        Hd1 = 1,
        /// <summary>全民营销-红包活动</summary>
        [Description("红包活动")]
        Hd2 = 2,
        /// <summary>暂定</summary>
        [Description("")]
        Hd3 = 3,
    }
}
