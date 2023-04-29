using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 活动规则类型
    /// </summary>
    public enum ActivityRuleType
    {    
        /// <summary>单篇奖金</summary>
        [Description("单篇奖金")]
        SingleBonus = 1,
        /// <summary>上线奖金</summary>
        [Description("上线奖金")]
        OnlineBonus = 2,
        /// <summary>第N篇额外奖金</summary>
        [Description("第N篇额外奖金")]
        ExtraBonus = 3,
        /// <summary>参加内容预计金额达到上限继续/停止活动(规则表中ActivityRule.Type=4时，当number=1则表示继续活动、number=2则表示停止活动)</summary>
        [Description("继续/停止活动")]
        StopOrKeepActivity = 4,       
        /// <summary>审核通过后N天内用户不能自行修改、删除评测</summary>
        [Description("审核通过后N天内不允许用户修改删除评测")]
        OperationNotAllowed = 5,
    }
}
