using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.EvaluationCrawler
{
    /// <summary>
    /// 抓取评测列表请求参数实体类
    /// </summary>
    public class CaptureEvaluationListQuery : IRequest<CrawlerListDto>
    {
        /// <summary>
        /// 类型
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }
    }
}
