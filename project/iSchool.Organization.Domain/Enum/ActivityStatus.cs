using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 活动状态
    /// </summary>
    public enum ActivityStatus
    {
        /// <summary>正常|审核成功|上架</summary>
        [Description("上架中")]
        Ok = 1,
        /// <summary>失败|审核失败|下架</summary>
        [Description("已下架")]
        Fail = 2,        
    }

    /// <summary>
    /// 活动状态（前端显示）
    /// </summary>
    public enum ActivityFrontStatus
    {
        /// <summary>正常|审核成功|上架|期间</summary>
        [Description("正常")]
        Ok = 1,
        /// <summary>失败|审核失败|下架</summary>
        [Description("已下架")]
        Fail = 2,
        /// <summary>未开始</summary>
        [Description("未开始")]
        NotStarted = 3,
        /// <summary>已过期</summary>
        [Description("已过期")]
        Expired = 4,
        /// <summary>不存在</summary>
        [Description("不存在")]
        NotExsits = 5,
        /// <summary>已删除</summary>
        [Description("已删除")]
        Deleted = 6,
        /// <summary>到达每日上限</summary>
        [Description("到达每日上限")]
        DayLimited = 7,
    }
}
