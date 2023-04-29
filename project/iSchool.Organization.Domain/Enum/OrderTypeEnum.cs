using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 订单类型
    /// </summary>
    public enum OrderType
    {
        /// <summary>认证课程购买</summary>        
        CourseBuy = 1,

        /// <summary>微信方式购买课程</summary>
        BuyCourseByWx = 2,

        /// <summary>积分购买（积分+金额购买）</summary>
        Ty3 = 3,
    }
}
