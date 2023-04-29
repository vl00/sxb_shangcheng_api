using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 活动评测提交状态
    /// </summary>
    public enum ActiEvltSubmitType
    {
        /// <summary>初次提交</summary>
        [Description("初次提交")]
        First = 1,
        /// <summary>驳回重交</summary>
        [Description("驳回重交")]
        Retrial = 2,
        /// <summary>多次提交</summary>
        [Description("多次提交")]
        Multi = 3,
    }

    /// <summary>
    /// 活动评测审核    
    /// </summary>
    public enum ActiEvltAuditStatus
    {
        /// <summary>待审核</summary>
        [Description("待审核")]
        Audit = 1,
        /// <summary>待审核(手机号冲突)</summary>
        [Description("待审核(手机号冲突)")]
        AuditButMoblieExcp = 2,
        /// <summary>通过</summary>
        [Description("通过")]
        Ok = 3,
        /// <summary>不通过</summary>
        [Description("不通过")]
        Failed = 4,        
        /// <summary>
        /// 脱离活动<br/>
        /// 因从活动专题改成非活动专题等而导致脱离活动审核流程
        /// </summary>
        [Description("非活动")]
        Not = 5,
    }
}
