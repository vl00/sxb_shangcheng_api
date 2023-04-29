using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 账号异常type
    /// </summary>
    public enum UserAccountInvalidType
    {
        /// <summary>账号正常</summary>
        [Description("账号正常")]
        Normal = 0,
        /// <summary>手机号异常</summary>
        [Description("手机号异常")]
        MobileExcp = 1,
        /// <summary>账号受限</summary>
        [Description("账号受限")]
        Limited = 2,
    }
}
