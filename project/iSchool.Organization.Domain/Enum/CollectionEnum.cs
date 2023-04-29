using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 收藏类型
    /// </summary>
    public enum CollectionEnum
    {        
        /// <summary>
        /// 课程
        /// </summary>
        [Description("课程")]
        Course=1,

        /// <summary>
        /// 评测
        /// </summary>
        [Description("评测")]
        Evaluation = 2,
    }
}
