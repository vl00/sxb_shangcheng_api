using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 专题关联评测
    /// </summary>
    public class SpecialEvlts
    {       
        /// <summary>序号</summary>
        public int RowNum { get; set; }

        /// <summary>评测Id</summary>
        public Guid Id { get; set; }

        /// <summary>评测标题</summary>
        public string Title { get; set; }

        /// <summary>发布时间</summary>
        public string CreateTime { get; set; }

        /// <summary>作者Id</summary>
        public Guid UserId { get; set; }

        /// <summary>(true:专题已关联该评测；false:专题未关联该评测)</summary>
        public bool IsCheck { get; set; }
    }
}
