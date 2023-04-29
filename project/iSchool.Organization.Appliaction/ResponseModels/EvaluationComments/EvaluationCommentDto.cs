#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public abstract class BaseEvaluationCommentDto
    {
        /// <summary>评论id</summary>
        public Guid Id { get; set; }
        /// <summary>评论者id</summary>
        public Guid UserId { get; set; }
        /// <summary>评论者名称</summary>
        public string Username { get; set; } = default!;
        /// <summary>评论者头像</summary>
        public string? UserImg { get; set; }
        /// <summary>评论时间</summary>
        public DateTime CreateTime { get; set; }
        /// <summary>服务器的当前时间</summary>
        public DateTime Now { get; set; }
        /// <summary>评论内容</summary>
        public string Comment { get; set; } = default!;
        /// <summary>点赞数</summary>
        public int Likes { get; set; }
        /// <summary>是否是我曾经点赞过的</summary>
        public bool IsLikeByMe { get; set; }
        /// <summary>是否是我发的评论</summary>
        public bool IsMy { get; set; }
        /// <summary>回复数量</summary>
        public int CommentCount { get; set; }

        /// <summary>作者ID </summary>
        public Guid AuthorId { get; set; }
        /// <summary>是否是评测作者</summary>
        public bool IsAuthor { get; set; }
    }

    /// <summary>
    /// 评测评论
    /// </summary>
    public class EvaluationCommentDto : BaseEvaluationCommentDto
    {
        /// <summary>作者回复的回复</summary>
        public SubCommentDto[]? SubComments { get; set; }
    }

    public class SubCommentDto
    {
        /// <summary>
        /// 评论id
        /// </summary>
        public Guid Id { get; set; }
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
        /// 评论内容
        /// </summary>
        public string Comment { get; set; } = default!;
        public bool IsAuthor { get; set; }

        /// <summary>创建时间</summary>
        public DateTime CreateTime { get; set; }
    }
}

#nullable disable