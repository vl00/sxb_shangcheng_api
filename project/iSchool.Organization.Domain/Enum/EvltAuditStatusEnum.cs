using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 评测审核状态
    /// </summary>
    public enum EvltAuditStatusEnum
    {
        /// <summary>未审核</summary>
        [Description("未审核")]
        UnAudit = 0,
        /// <summary>审核通过</summary>
        [Description("审核通过")]
        Ok = 1,
        /// <summary>审核不通过</summary>
        [Description("审核不通过")]
        Failed = 2,
    }
}
