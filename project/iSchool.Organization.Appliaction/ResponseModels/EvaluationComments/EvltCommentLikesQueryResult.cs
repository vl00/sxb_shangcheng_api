#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 评测评论点赞查询结果
    /// </summary>
    public class EvltCommentLikesQueryResult
    {
        /// <summary>
        /// Likecount: 真实点赞+虚拟点赞.<br/>
        /// IsLikeByMe: 是否是我点赞过,未登录用户为false.<br/>
        /// </summary>
        public Dictionary<(Guid EvltId, Guid CommentId), (int Likecount, bool IsLikeByMe)> Items { get; set; } = default!;
    }
}

#nullable disable