using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 兑换码状态
    /// </summary>
    public enum ExchangeStatus
    {
        /// <summary>已兑换|发送成功</summary>
        [Description("已兑换")]
        Converted = 1,
        /// <summary>未使用</summary>
        [Description("未使用")]
        NotUsed = 2,
        /// <summary>发送失败</summary>
        [Description("发送失败")]
        Fail_In_Send = 3,        

    }

   
}
