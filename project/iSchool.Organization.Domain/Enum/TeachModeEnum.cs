using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 教学模式/上课方式
    /// </summary>
    public enum TeachModeEnum
    {
        /// <summary>
        /// 直播
        /// </summary>
        [Description("线上")]
        DirectLive = 1,

        /// <summary>
        /// 录播
        /// </summary>
        [Description("教程")]
        Recorded = 2,

        /// <summary>
        /// 互动
        /// </summary>
        [Description("互动")]
        Interaction = 3,

        /// <summary>
        /// 资源平台
        /// </summary>
        [Description("资源平台")]
        ResourcePlatform = 4,

        /// <summary>
        /// 其他
        /// </summary>
        [Description("其他")]
        Other = 99,
    }
}
