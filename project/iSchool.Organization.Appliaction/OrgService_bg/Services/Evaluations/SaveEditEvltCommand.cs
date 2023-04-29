
using iSchool.Organization.Appliaction.RequestModels.EvaluationCrawler;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    /// <summary>
    /// 保存评测
    /// </summary>
    public class SaveEditEvltCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测Id(如果新建评测，则new)
        /// </summary>
        [Description("Id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 标题
        /// </summary>
        [Description("Title")]
        public string Title { get; set; }

        /// <summary>
        /// 正文集合（key=evltitem.Type）
        /// </summary>
        [Description("Content")]
        public Dictionary<int, string> DicContent { get; set; }

        /// <summary>
        /// 图片集合（key=evltitem.Type）
        /// </summary>
        [Description("Url")]
        public Dictionary<int, string> DicUrl { get; set; }

        /// <summary>
        /// 缩略图片集合（key=evltitem.Type）
        /// </summary>
        [Description("ThumUrl")]
        public Dictionary<int, string> DicThumUrl { get; set; }

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
    }
}
