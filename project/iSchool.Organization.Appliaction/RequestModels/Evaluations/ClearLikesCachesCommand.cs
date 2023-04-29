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
    /// 清除点赞缓存s cmd
    /// </summary>
    public class ClearLikesCachesCommand : IRequest
    {
        /// <summary>
        /// when Type=1 评测s id <br/>
        /// when Type=2 评论s id <br/>
        /// </summary>
        public IEnumerable<Guid> Ids { get; set; } = default!;
        /// <summary>
        /// 跟数据库dbo.[Like]表type字段一样<br/>        
        /// </summary>
        public byte Type { get; set; } = 1;
        /// <summary>超时,默认5s</summary>
        public int TimeoutSeconds { get; set; } = 5;
    }

#nullable disable
}
