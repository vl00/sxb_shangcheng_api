
using iSchool.Organization.Appliaction.RequestModels.EvaluationCrawler;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.ComponentModel;

namespace iSchool.Organization.Appliaction.Service.EvaluationCrawler
{
    /// <summary>
    /// 发布抓取评测
    /// </summary>
    public class ReleaseCaptureEvaluationCommand: IRequest<ResponseResult>
    {
        /// <summary>
        /// 抓取评测Id
        /// </summary>
        [Description("Id")]
        public Guid Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Description("Title")]
        public string Title { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        [Description("Content")]
        public string Content { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        [Description("Url")]
        public string Url { get; set; }

        /// <summary>
        /// 缩略图片
        /// </summary>
        [Description("ThumUrl")]
        public string ThumUrl { get; set; }

        /// <summary>
        /// 专题
        /// </summary>
        [Description("Specialid")]
        public Guid? Specialid { get; set; }

        /// <summary>
        /// 课程机构
        /// </summary>
        [Description("OrgId")]
        public Guid? OrgId { get; set; }

        /// <summary>
        /// 课程
        /// </summary>
        [Description("CourseId")]
        public Guid? CourseId { get; set; }

        ///// <summary>
        ///// 年龄段
        ///// </summary>
        //[Description("Age")]
        //public int? Age { get; set; }

        ///// <summary>
        ///// 上课方式
        ///// </summary>
        //[Description("Mode")]
        //public string Mode { get; set; }

        ///// <summary>
        ///// 课程周期
        ///// </summary>
        //[Description("Cycle")]
        //public string Cycle { get; set; }

        ///// <summary>
        ///// 课程价格
        ///// </summary>
        //[Description("Price")]
        //public decimal? Price { get; set; }

        /// <summary>
        /// 评论  json格式
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// 是否紧急发布，(true:紧急发布；默认false)
        /// </summary>
        public bool IsUrgent { get; set; } = false;

        /// <summary>
        /// 发布者
        /// </summary>
        public Guid? Creator { get; set; }
    }
}
