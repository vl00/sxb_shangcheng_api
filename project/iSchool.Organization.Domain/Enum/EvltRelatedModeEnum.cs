using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 关联主体
    /// </summary>
    public enum EvltRelatedModeEnum
    {
        /// <summary>课程</summary>
        [Description("课程")]
        Course = 1,
        /// <summary>品牌</summary>
        [Description("品牌")]
        Org = 2,
        /// <summary>其他</summary>
        [Description("其他")]
        Other = 3,
    }
}
