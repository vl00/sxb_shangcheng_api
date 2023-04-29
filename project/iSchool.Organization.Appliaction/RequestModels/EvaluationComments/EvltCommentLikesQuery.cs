using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 查询评测评论点赞
    /// </summary>
    public class EvltCommentLikesQuery : IRequest<EvltCommentLikesQueryResult>
    {
        public (Guid EvltId, Guid CommentId)[] Ids { get; set; } = default!;
    }

#nullable disable
}
