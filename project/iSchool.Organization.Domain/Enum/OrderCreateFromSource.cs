using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 下订单来源类型
    /// </summary>
    public enum OrderCreateFromSource
    {
        /// <summary>微信合作院校库</summary>        
        [Description("微信合作院校库")]
        SchoolFromWx = 1,

        /// <summary>其它</summary>        
        [Description("其它")]
        Other = 99,
    }
}
