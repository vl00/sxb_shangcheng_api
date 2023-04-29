using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 评测添加评论
    /// </summary>
    public class AddEvltCommentCommand : IRequest<AddEvltCommentDto>
    { 
        /// <summary>
        /// 评测id
        /// </summary>
        [Required]
        public Guid EvltId { get; set; }
        /// <summary>
        /// 评论内容
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 评测评论ID
        /// </summary>
        public Guid? EvltCommentId { get; set; }
    }
}
