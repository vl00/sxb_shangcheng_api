using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.Lables
{
    /// <summary>
    /// 评测卡片实体
    /// </summary>
    public class EvalDetailsLable
    {
        /// <summary>
        /// 评测长Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 评测短Id
        /// </summary>
        public string Id_s { get; set; }

        /// <summary>
        /// 评测标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 评测内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 评测封面图
        /// </summary>
        public string CoverUrl { get; set; }
    }
}
