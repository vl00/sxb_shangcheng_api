using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 年龄段枚举
    /// </summary>
    public enum AgeGroup
    {
        /// <summary>
        /// 0-3
        /// </summary>
        [Description("0-3")]
        AgeGroup1 = 1,
        /// <summary>
        /// 3-6
        /// </summary>
        [Description("3-6")]
        AgeGroup2 = 2,
        /// <summary>
        /// 6-12
        /// </summary>
        [Description("6-12")]
        AgeGroup3 = 3,
        /// <summary>
        /// 12-15
        /// </summary>
        [Description("12-15")]
        AgeGroup4 = 4,
        /// <summary>
        /// 15-18
        /// </summary>
        [Description("15-18")]
        AgeGroup5 = 5,

        //[Description("其他")]
        //AgeGroupOther =100
    }
}
