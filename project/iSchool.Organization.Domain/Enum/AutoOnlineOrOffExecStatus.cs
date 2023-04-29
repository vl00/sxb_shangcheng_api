using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 自动上下线执行状态
    /// </summary>
    public enum AutoOnlineOrOffExecStatus
    {
        /// <summary>未执行</summary>
        [Description("未执行")]
        Todo = 0,
        /// <summary>成功</summary>
        [Description("成功")]
        Sucessed = 1,
        /// <summary>失败</summary>
        [Description("失败")]
        Failed = 2,
    }
}
