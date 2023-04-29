using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 佣金锁定期类型
    /// </summary>
    public enum NolimitTypeEnum
    {
        /// <summary>
        /// 具体日期
        /// </summary>
        [Description("具体日期")]
        ExactDate = 1,
        /// <summary>
        /// 购买后N天
        /// </summary>
        [Description("购买后N天")]
        NDaysLater = 2,
        /// <summary>
        /// 不锁定（立即可提现）
        /// </summary>
        [Description("不锁定（立即可提现）")]
        NotLocked = 3,
    }
}
