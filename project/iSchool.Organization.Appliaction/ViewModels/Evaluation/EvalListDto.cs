using iSchool.Organization.Appliaction.Service.Evaluations;
using iSchool.Organization.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 评测列表
    /// </summary>
    public class EvalListDto
    {
        public List<EvalItem> list { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
    }

    public class EvalItem
    {
        #region 不展示
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvalId { get; set; }

        /// <summary>
        /// 评测绑定表Id，用于更新科目
        /// </summary>
        public Guid BId { get; set; }        

        #endregion

        /// <summary>
        /// 序号
        /// </summary>
        public int RowNum { get; set; }

        /// <summary>
        ///标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string Cover { get; set; }

        /// <summary>
        /// 视频第一帧
        /// </summary>
        public string VideoCover { get; set; }

        /// <summary>
        /// UV--用户浏览数
        /// </summary>
        public int UV { get; set; } = 0;

        /// <summary>
        /// 用于查UV
        /// </summary>
        public string No { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int? CommentCount { get; set; }

        /// <summary>
        /// 是否官方(true:是；false:否)
        /// </summary>
        public bool IsOfficial { get; set; }

        /// <summary>
        /// 是否加精（true：是；false：否）
        /// </summary>
        public bool Stick { get; set; }

        ///// <summary>
        ///// 相关科目
        ///// </summary>
        //public int? Subject { get; set; }

        /// <summary>
        /// 课程科目
        /// </summary>
        public string Subjects { get; set; }

        /// <summary>
        ///是否纯文字图片（true：是；false：否）
        /// </summary>
        public bool IsPlaintext { get; set; }

        /// <summary>
        /// 状态(1：界面展示下架；其他展示下架)
        /// </summary>
        public byte Status { get; set; }

        /// <summary>课程集合</summary>
        public List<RelatedBodyInfo> ListCourses { get; set; }
        /// <summary>品牌s</summary>
        public List<RelatedBodyInfo> ListOrgs { get; set; }

        /// <summary>
        /// 关联课程名称
        /// </summary>
        public string CourseTitle { get; set; }

        /// <summary>
        /// 真赞
        /// </summary>
        public int Likes { get; set; }

        /// <summary>
        /// 官赞
        /// </summary>
        public int ShamLikes { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 审核状态(1：审核通过；2：审核不通过)
        /// </summary>
        public int? AuditStatus { get; set; }

        public Guid? AuditorId { get; set; }
        public string AuditorName { get; set; }

        /// <summary>
        /// 素材下载次数
        /// </summary>
        public int? DownloadMaterialCount { get; set; }

        public DateTime CreateTime { get; set; }
    }


    /// <summary>
    /// 评论列表
    /// </summary>
    public class EvltCommentListDto 
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }

        public List<EvltCommentItem> list { get; set; }
    }

    /// <summary>
    /// 评论
    /// </summary>
    public class EvltCommentItem 
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int RowNum { get; set; }

        /// <summary>
        /// 是否官方(true:是；false:否)
        /// </summary>
        public bool IsOfficial { get; set; }

        /// <summary>
        /// 评论Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>
        public string Comment { get; set; }

    }


}
