using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 评测内容模式枚举
    /// </summary>
    public enum EvltContentModeEnum
    {
        /// <summary>
        /// 自由模式
        /// </summary>
        [Description("自由模式")]
        Normal = 1,
        /// <summary>
        /// 专业模式
        /// </summary>
        [Description("专业模式")]
        Pro = 2,
    }
}
