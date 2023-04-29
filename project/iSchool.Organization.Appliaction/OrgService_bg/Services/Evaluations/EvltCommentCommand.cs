using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;


namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 更新评论
    /// </summary>
    public class EvltCommentCommand:IRequest<ResponseResult>
    {
        /// <summary>
        /// 评论Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvltId { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>
        public string Comment { get; set; }
    }
}
