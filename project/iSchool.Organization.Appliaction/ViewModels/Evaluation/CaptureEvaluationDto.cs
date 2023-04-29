using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 抓取评测详情--返回实体
    /// </summary>
    public class CaptureEvaluationDto
    {

        public Guid Id { get; set; }

        /// <summary>
        /// 状态（0:初始化；1:已编辑；2:已发布；）
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// 抓取类型
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// 抓取来源/关键词
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 图片（json格式）
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 专题Id
        /// </summary>
        public Guid? Specialid { get; set; }

        #region 课程信息

        /// <summary>
        /// 课程机构（机构下拉框）
        /// </summary>
        public Guid? OrgId { get; set; }

        /// <summary>
        /// 课程（课程下拉框）
        /// </summary>
        public Guid? CourseId { get; set; }

        /// <summary>
        /// 年龄段
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// 上课方式
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// 课程周期
        /// </summary>
        public string Cycle { get; set; }

        /// <summary>
        /// 课程价格
        /// </summary>
        public decimal? Price { get; set; }

        #endregion

        /// <summary>
        /// //抓取评论(评论:json格式)
        /// </summary>
        public string Comments { get; set; }

    }
}
