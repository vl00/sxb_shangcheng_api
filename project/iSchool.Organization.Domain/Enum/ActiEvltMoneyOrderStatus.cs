using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 活动评测订单状态 <br/>
    /// 除以3余数为0的是为'已支出'
    /// </summary>
    public enum ActiEvltMoneyOrderStatus
    {
        ///// <summary>未定义</summary>
        //[Description("未定义")]
        //None = 0,

        /// <summary>已支出(走流程)</summary>
        [Description("已支出")]
        Ok = 3,
        /// <summary>已支出(线下手动)</summary>
        [Description("线下已支出")]
        PayedManually = 6,

        /** ??
              未定义 | 无要求
              Created 已创建 | 确定要支出
              待支付
              Processing 支出请求中
              已支出 | 手动支出
              支付超时
              取消支付
              冻结
         */
    }
}
