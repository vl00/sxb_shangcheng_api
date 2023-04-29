using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using MediatR;

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 评论
    /// </summary>
    public class EvltCommentByEvltIdQuery:IRequest<PagedList<EvltCommentItem>>
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvltId { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; } = 5;
    }
}
