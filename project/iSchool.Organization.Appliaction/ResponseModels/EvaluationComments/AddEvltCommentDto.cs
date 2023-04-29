using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
  
    public class AddEvltCommentDto
    {
        /// <summary>
        /// 作者ID 
        /// </summary>
        public Guid AuthorId { get; set; }
        /// <summary>
        /// 评论id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 评论者id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// 评论者名称
        /// </summary>
        public string Username { get; set; } = default!;
        /// <summary>
        /// 评论者头像
        /// </summary>
        public string? UserImg { get; set; }
        /// <summary>
        /// 评论时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 服务器的当前时间
        /// </summary>
        public DateTime Now { get; set; }
        /// <summary>
        /// 评论内容
        /// </summary>
        public string Comment { get; set; } = default!;

        /// <summary>
        /// 是否是评测作者
        /// </summary>
        public bool IsAuthor { get; set; }
        /// <summary>
        /// 点赞数量
        /// </summary>
        public int Likes { get; set; }
    }
}
