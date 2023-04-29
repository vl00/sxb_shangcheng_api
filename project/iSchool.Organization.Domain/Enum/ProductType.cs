using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// (orderdetial)产品类型
    /// </summary>
    public enum ProductType
    {
        /// <summary>课程</summary>
        [Description("课程")]
        Course = 1,
        /// <summary>好物</summary>
        [Description("好物")]
        Goodthing = 2,
    }
}
