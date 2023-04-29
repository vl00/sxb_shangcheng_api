using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 抓取评测返回实体类
    /// </summary>
    public class CrawlerListDto
    {
        /// <summary>
        /// 抓取评测列表
        /// </summary>
        public List<CrawlerItem> list { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }

    public class CrawlerItem
    {
        #region 不展示
        /// <summary>
        /// 抓取评测Id
        /// </summary>
        public Guid Id { get; set; } 
        #endregion

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }       

        /// <summary>
        /// 来源
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// 抓取时间
        /// </summary>
        public DateTime CreateTime { get; set; }       
       
    }

}
