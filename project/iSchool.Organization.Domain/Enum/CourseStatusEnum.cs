using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 课程状态枚举
    /// </summary>
    public enum CourseStatusEnum
    {
        ///// <summary>
        ///// 用户刚创建,相当于待审核.
        ///// </summary>
        //[Description("刚创建")]
        //Inited = 0,
        /// <summary>
        /// 正常|审核成功|上架
        /// </summary>
        [Description("上架")]
        Ok = 1,
        /// <summary>
        /// 失败|审核失败|下架
        /// </summary>
        [Description("下架")]
        Fail = 0,
    }
}
