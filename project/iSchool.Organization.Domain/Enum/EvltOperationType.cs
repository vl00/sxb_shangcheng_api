using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.Enum
{
    /// <summary>
    /// 评测操作类型
    /// </summary>
    public enum EvltOperationType
    {
        /// <summary>
        /// 新增
        /// </summary>
        [Description("新增")]
        Add = 1,

        /// <summary>
        /// 全面修改(通过编辑按钮的修改)
        /// </summary>
        [Description("全面修改")]
        BigUpdate = 2,

        /// <summary>
        /// 官赞修改
        /// </summary>
        [Description("官赞修改")]
        LikesUpdate = 3,

        /// <summary>
        /// 不影响评论的修改(上下架/是否加精/是否纯文字图片/相关科目)
        /// </summary>
        [Description("不影响评论的修改")]
        Update = 4,
    }
}
