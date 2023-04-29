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
    /// 分页查询某个评测里的评论列表
    /// </summary>
    public class EvltCommentsQuery : IRequest<PagedList<EvaluationCommentDto>>
    {
     
        /// <summary>评测id</summary>
        public Guid EvltId { get; set; }
        /// <summary>页码</summary>
        public int PageIndex { get; set; }
        /// <summary>页大小，默认10</summary>
        public int PageSize { get; set; } = 10;
        /// <summary>不超过给出的时间</summary>
        public DateTime? Naf { get; set; }

        public bool AllowFindChilds { get; set; } = true;
    }

#nullable disable
}
