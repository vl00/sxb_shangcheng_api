using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// all专题列表项
    /// </summary>
    public class SimpleSpecialDto
    {
        /// <summary>专题id</summary>
        public Guid Id { get; set; }
        /// <summary>标题</summary>
        public string Title { get; set; }
        /// <summary>专题短id</summary>
        public string Id_s { get; set; }
        /// <summary>专题图片</summary>
        public string Banner { get; set; }
        /// <summary>活动号.可null</summary>
        public string Acode { get; set; } = null;
        /// <summary>
        /// 活动类型<br/>        
        /// ```
        /// /// <summary>未定义</summary>
        /// [Description("未定义")]
        /// None = 0,
        /// /// <summary>旧活动</summary>
        /// [Description("旧活动")]
        /// Hd1 = 1,
        /// /// <summary>全民营销-红包活动</summary>
        /// [Description("红包活动")]
        /// Hd2 = 2,
        /// ```
        /// </summary>        
        public int? Atype { get; set; }
    }
}
