using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.EvaluationComments
{
    /// <summary>
    /// 我的评论
    /// </summary>
    public class MyEvltCommentDto
    {
        public Guid EvaluationId { get; set; }
        /// <summary>
        /// 评论id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 评测短id
        /// </summary>
        public string Id_s { get; set; }
        /// <summary>
        /// 评论者id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// 评论者名称
        /// </summary>
        public string Username { get; set; } = default!;
        /// <summary>
        /// 评论者头像
        /// </summary>
        public string? UserImg { get; set; }
        /// <summary>
        /// 评论时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 服务器的当前时间
        /// </summary>
        public DateTime Now { get; set; }
        /// <summary>
        /// 评论内容
        /// </summary>
        public string Comment { get; set; } = default!;
        /// <summary>
        /// 点赞数
        /// </summary>
        public int Likes { get; set; }
        /// <summary>
        /// 软删除状态
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// 如果是回复，对应的父评论ID
        /// </summary>
        public Guid FromId { get; set; }
        /// <summary>
        /// 0 代表评测评论  1代表回复
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 被评论内容
        /// </summary>
        public string TargetContent { get; set; }
        /// <summary>
        /// 被评论内容图片
        /// </summary>
        public string TargetImgLink { get; set; }
        /// <summary>
        /// 被评论对象状态。1正常。0删除。如果状态为删除。前端显示原内容已被删除
        /// </summary>
        public bool TargetValid { get; set; }
        /// <summary>
        /// 被评论对象标题 
        /// </summary>
        public string TargetTitle { get; set; }
        /// <summary>
        /// 被评论对象状态1:正常，2:下架
        /// </summary>

        public int TargetStatus { get; set; }

    }
}
