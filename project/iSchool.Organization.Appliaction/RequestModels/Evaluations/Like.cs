using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 点赞评测
    /// </summary>
    public class LikeEvaluationCommand : IRequest<bool>
    {
        /// <summary>
        /// 评测id
        /// </summary>
        public Guid EvaluationId { get; set; }
        /// <summary>
        /// true=点赞;false=取消点赞
        /// </summary>
        public bool IsLike { get; set; }
    }

    /// <summary>
    /// 点赞评测里评论
    /// </summary>
    public class LikeEvaluationCommentCommand : IRequest
    {
        /// <summary>
        /// 评论id
        /// </summary>
        public Guid EvltCommentId { get; set; }

        /// <summary>
        /// 评测id
        /// </summary>
        public Guid EvaluationId { get; set; }
        /// <summary>
        /// true=点赞;false=取消点赞
        /// </summary>
        public bool IsLike { get; set; }
    }
}
