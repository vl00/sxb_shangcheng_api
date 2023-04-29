using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 分页查询回复
    /// </summary>
    public class ChildrenCommentsQuery : IRequest<PagedList<EvaluationCommentDto>>
    {
        /// <summary>
        /// 评测ID
        /// </summary>
        [Required]
        public Guid EvltId { get; set; }
        /// <summary>
        /// 评测评论ID 
        /// </summary>
        public Guid EvltCommentId { get; set; }
        /// <summary>页码</summary>
        public int PageIndex { get; set; }
        /// <summary>页大小，默认20</summary>
        public int PageSize { get; set; } = 20;
        /// <summary>不超过给出的时间</summary>
        public DateTime? Naf { get; set; }
    }

#nullable disable
}
