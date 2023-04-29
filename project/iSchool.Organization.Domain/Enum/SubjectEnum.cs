using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 科目枚举
    /// </summary>
    public enum SubjectEnum
    {
        /// <summary>
        /// 语文
        /// </summary>
        [Description("语文")]
        Chinese = 101 ,
        /// <summary>
        /// 数学
        /// </summary>
        [Description("数学")]
        Math = 102 ,
        /// <summary>
        /// 英语
        /// </summary>
        [Description("英语")]
        English =103 ,
        /// <summary>
        /// Steam
        /// </summary>
        [Description("Steam")]
        Steam = 104,
        /// <summary>
        /// 绘画
        /// </summary>
        [Description("绘画")]
        Draw = 105,
        /// <summary>
        /// 音乐
        /// </summary>
        [Description("音乐")]
        Music = 106,
        /// <summary>
        /// 思维
        /// </summary>
        [Description("思维")]
        Thought = 107,
        /// <summary>
        /// 编程
        /// </summary>
        [Description("编程")]
        Programming = 108,
        /// <summary>
        /// 科学
        /// </summary>
        [Description("科学")]
        Science = 109,
        /// <summary>
        /// 棋类
        /// </summary>
        [Description("棋类")]
        ChessAndCards = 110,
        /// <summary>
        /// 早教
        /// </summary>
        [Description("早教")]
        EarlyEducation = 111,
        /// <summary>
        /// 综合素养
        /// </summary>
        [Description("综合素养")]
        Enm112 = 112,
        /// <summary>
        /// 其他
        /// </summary>
        [Description("其他")]
        Other = 199
    }
}
