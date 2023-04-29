using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 机构分类(用于Organization表types字段)
    /// </summary>
    public enum OrgCfyEnum
    {
        /// <summary>
        /// 语文
        /// </summary>
        [Description("语文")]
        Chinese = 1,
        /// <summary>
        /// 数学
        /// </summary>
        [Description("数学")]
        Math = 2,
        /// <summary>
        /// 英语
        /// </summary>
        [Description("英语")]
        English = 3,
        /// <summary>
        /// Steam
        /// </summary>
        [Description("Steam")]
        Steam = 4,
        /// <summary>
        /// 绘画
        /// </summary>
        [Description("绘画")]
        Draw = 5,
        /// <summary>
        /// 音乐
        /// </summary>
        [Description("音乐")]
        Music = 6,
        /// <summary>
        /// 思维
        /// </summary>
        [Description("思维")]
        Thought = 7,
        /// <summary>
        /// 多学科
        /// </summary>
        [Description("多学科")]
        MultiSubject = 8,      
        /// <summary>
        /// 其他
        /// </summary>
        [Description("其他")]
        Other = 99,
    }
}
