using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 是否类型
    /// </summary>
    public enum CooperationStatus
    {
        /// <summary>
        /// 是
        /// </summary>
        [Description("是")]
        Authentication=1,

        /// <summary>
        /// 否
        /// </summary>
        [Description("否")]
        UnAuthentication = 0,
    }
}
