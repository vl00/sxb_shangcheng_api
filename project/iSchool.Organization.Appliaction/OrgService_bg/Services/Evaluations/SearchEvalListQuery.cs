using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    /// <summary>
    /// 评测管理-列表
    /// </summary>
    public class SearchEvalListQuery : IRequest<EvalListDto>
    {
        /// <summary>
        /// 开始时间(发表)
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间(发表)
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 相关科目
        /// </summary>
        public int? Subject { get; set; }

        /// <summary>
        /// 关联课程
        /// </summary>
        public Guid? CourseId { get; set; }

        /// <summary>
        /// 是否加精（true：是；false：否）
        /// </summary>
        public bool? Stick { get; set; }

        /// <summary>
        ///是否纯文字图片（true：是；false：否）
        /// </summary>
        public bool? IsPlaintext { get; set; }

        /// <summary>
        /// 是否官方评测
        /// </summary>
        public bool? IsOfficial { get; set; }

        /// <summary>
        /// 查询字段
        /// </summary>
        public string SearchField { get; set; }

        /// <summary>
        /// 查询字段值
        /// </summary>
        public string SearchFieldValue { get; set; }

        /// <summary>
        /// 上架状态(true：上架；false：下架；默认为null,查全部)
        /// </summary>
        public bool? IsOnTheShelf { get; set; }

        /// <summary>
        /// 审核状态(1：审核通过；2：审核不通过)
        /// </summary>
        public int? AuditStatus { get; set; }

        public string Mobile { get; set; }
        public string UserName { get; set; }

        /// <summary>关联主体</summary>
        public string RelatedBody { get; set; }

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
