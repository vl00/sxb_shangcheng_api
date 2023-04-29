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
    /// 回复列表(不分页)
    /// </summary>
    public class ReplyByComIdQuery : IRequest<List<EvltCommentItem>>
    {
        /// <summary>
        /// 评论Id
        /// </summary>
        public Guid ComId { get; set; }
    }
}
