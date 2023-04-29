using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 积分类型
    /// </summary>
    public enum CourseExchangeTypeEnum
    {
        /// <summary>
        /// 被发展人购买资格------>付费机会制
        /// </summary>
        [Description("付费机会制")]
        Ty1 = 1,
        /// <summary>
        /// 发展人购买资格积分------>推广积分制
        /// </summary>
        [Description("推广积分制")]
        Ty2 = 2,
        /// <summary>
        /// 平台积分制
        /// </summary>
        [Description("平台积分制")]
        Ty3 = 3,
    }
}
