using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 用于Course表type字段
    /// </summary>
    public enum CourseTypeEnum
    {
        /// <summary>
        /// 课程
        /// </summary>
        [Description("课程")]
        Course = 1,
        /// <summary>
        /// 好物
        /// </summary>
        [Description("好物")]
        Goodthing = 2,
    }
}
