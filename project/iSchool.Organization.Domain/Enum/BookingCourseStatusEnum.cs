using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 约课状态枚举
    /// </summary>
    public enum BookingCourseStatusEnum
    {
        /// <summary>
        /// 待排课
        /// </summary>
        [Description("待排课")]
        WaitFor = 1,
        /// <summary>
        /// 排课中
        /// </summary>
        [Description("排课中")]
        Arrangementing = 2,
        /// <summary>
        /// 已排课
        /// </summary>
        [Description("已排课")]
        Arranged = 3,
        /// <summary>
        /// 完 课
        /// </summary>
        [Description("完 课")]
        Finished = 4,
        /// <summary>
        /// 无
        /// </summary>       
        [Description("无")]
        Nothing = 99
    }
}
