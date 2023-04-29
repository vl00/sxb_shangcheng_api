#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 评测详情 -- base info
    /// </summary>
    public class EvltBaseInfoDto
    {
        /// <summary>
        /// 评测id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 评测no
        /// </summary>
        public long No { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = default!;
        /// <summary>
        /// 是否置顶/精华
        /// </summary>
        public bool Stick { get; set; }        
        /// <summary>
        /// 是否没图
        /// </summary>
        public bool IsPlaintext { get; set; }
        /// <summary>
        /// 是否有视频
        /// </summary>
        public bool HasVideo { get; set; }
        /// <summary>
        /// 封面图
        /// </summary>
        public string Cover { get; set; } = default!;

        /// <summary>
        /// 作者id
        /// </summary>
        public Guid AuthorId { get; set; }
        ///// <summary>
        ///// 作者名
        ///// </summary>
        //public string AuthorName { get; set; } = default!;
        ///// <summary>
        ///// 作者头像
        ///// </summary>
        //public string? AuthorHeadImg { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Parse("1986-06-01");
        /// <summary>
        /// 编辑时间(可为null)
        /// </summary>
        public DateTime? Mtime { get; set; }

        /// <summary>
        /// 专题id
        /// </summary>
        public Guid? SpecialId { get; set; }
        /// <summary>
        /// 专题短id
        /// </summary>
        public long? SpecialNo { get; set; }
        /// <summary>
        /// 专题名称
        /// </summary>
        public string? SpecialName { get; set; }

        /// <summary>
        /// 1=自由模式 2=专业模式
        /// </summary>
        public byte Mode { get; set; }
        
        /// <summary>
        /// 收藏数
        /// </summary>
        public int CollectionCount { get; set; }
        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }
        /// <summary>
        /// 点赞数
        /// </summary>
        public int LikeCount { get; set; }
        public int ViewCount { get; set; }
    }    
}

#nullable disable