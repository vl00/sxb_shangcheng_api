﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 评测列表项. 有图:无图 == 4:1 需要2个分页
    /// </summary>
    public class EvaluationItemDto
    {
        /// <summary>
        /// 评测id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 评测短id
        /// </summary>
        public string Id_s { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 是否置顶/精华
        /// </summary>
        public bool Stick { get; set; }
        /// <summary>
        /// 是否是纯文字
        /// </summary>
        public bool IsPlaintext { get; set; }
        /// <summary>
        /// 评测的封面图(缩略图)
        /// </summary>
        public string Cover { get; set; }

        /// <summary>
        /// 作者id
        /// </summary>
        public Guid AuthorId { get; set; }
        /// <summary>
        /// 作者名
        /// </summary>
        public string AuthorName { get; set; }
        /// <summary>
        /// 作者头像
        /// </summary>
        public string AuthorHeadImg { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Parse("1986-06-01");

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

        /// <summary>是否是我曾经点赞过的</summary>
        public bool IsLikeByMe { get; set; }

        /// <summary>
        /// 访问数
        /// </summary>
        public int ViewCount { get; set; }
    }
}
