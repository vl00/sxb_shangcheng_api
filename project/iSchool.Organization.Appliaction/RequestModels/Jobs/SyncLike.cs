using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 同步评测点赞
    /// </summary>
    public class SyncEvltLikeCommand : IRequest
    { 
        public TimeSpan Ttl { get; set; }
    }

    /// <summary>
    /// 同步评测里的评论点赞
    /// </summary>
    public class SyncEvltCommentLikeCommand : IRequest
    {
        public TimeSpan Ttl { get; set; }
    }
}
