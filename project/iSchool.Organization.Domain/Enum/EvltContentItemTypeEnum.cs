using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 评测内容item type枚举
    /// </summary>
    public enum EvltContentItemTypeEnum
    {
        /// <summary>
        /// 自由模式
        /// </summary>
        [Description("自由模式")]
        Normal = 0,
        /// <summary>
        /// 维度一
        /// </summary>
        [Description("维度一")]
        WeiDu1 = 1,
        /// <summary>
        /// 维度二
        /// </summary>
        [Description("维度二")]
        WeiDu2 = 2,
        /// <summary>
        /// 维度三
        /// </summary>
        [Description("维度三")]
        WeiDu3 = 3,
        /// <summary>
        /// 维度四
        /// </summary>
        [Description("维度四")]
        WeiDu4 = 4,
    }
}
