using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 抓取类型枚举
    /// </summary>
    public enum GrabTypeEnum
    {
        /// <summary>
        /// 链接
        /// </summary>
        [Description("链接")]
        Link = 1,
        /// <summary>
        /// 小红书搜索
        /// </summary>
        [Description("小红书搜索")]
        RedBookSearch = 2,
        /// <summary>
        /// 小红书专题
        /// </summary>
        [Description("小红书专题")]
        RedBookSpecial = 3,
        /// <summary>
        /// 个人主页
        /// </summary>
        [Description("个人主页")]
        PersonalHomePage = 4,        
    }

    /// <summary>
    /// 抓取评测状态
    /// </summary>
    public enum CaptureEvalStatusEnum
    {
        /// <summary>
        /// 初始化
        /// </summary>
        [Description("初始化")]
        Init = 1,
        /// <summary>
        /// 已编辑
        /// </summary>
        [Description("已编辑")]
        Edited = 2,
        /// <summary>
        /// 已发布
        /// </summary>
        [Description("已发布")]
        Published = 3,
        
    }
}
