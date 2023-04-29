#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 评测评论
    /// </summary>
    public class PcEvaluationCommentDto : BaseEvaluationCommentDto
    {
        /// <summary>作者回复的回复</summary>
        public PcSubCommentDto[]? SubComments { get; set; }
    }

    /// <summary>作者回复的回复</summary>
    public class PcSubCommentDto
    {
        /// <summary>评论id</summary>
        public Guid Id { get; set; }
        /// <summary>评论者id</summary>
        public Guid UserId { get; set; }
        /// <summary>评论者名称</summary>
        public string Username { get; set; } = default!;
        /// <summary>评论者头像</summary>
        public string? UserImg { get; set; }
        /// <summary>内容</summary>
        public string Comment { get; set; } = default!;
        public bool IsAuthor { get; set; } = true;

        /// <summary>创建时间</summary>
        public DateTime CreateTime { get; set; }

        /// <summary>点赞数</summary>
        public int Likes { get; set; }
        /// <summary>是否是我曾经点赞过的</summary>
        public bool IsLikeByMe { get; set; }
        /// <summary>是否是我发的</summary>
        public bool IsMy { get; set; }
    }
}

#nullable disable