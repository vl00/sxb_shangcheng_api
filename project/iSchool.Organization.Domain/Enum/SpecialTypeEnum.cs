using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 专题类型枚举
    /// </summary>
    public enum SpecialTypeEnum
    {
        /// <summary>
        /// 小专题
        /// </summary>
        [Description("小专题")]
        SmallSpecial = 1,
        /// <summary>
        /// 大专题
        /// </summary>
        [Description("大专题")]
        BigSpecial = 2,
    }
}
