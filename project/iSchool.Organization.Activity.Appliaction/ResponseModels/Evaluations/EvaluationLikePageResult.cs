using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Activity.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// （活动首页）评测排行列表分页数据.<br/>
    /// 通常不是第一页时只有PageInfo不为null
    /// </summary>
    public class EvaluationLikePageResult
    {
        /// <summary>分页信息,可null</summary>
        public PagedList<Organization.Appliaction.ResponseModels.EvaluationItemDto>? PageInfo { get; set; }
        /// <summary>海报</summary>
        public string? Banner { get; set; }
        /// <summary>我刚刚发布的评测item(不为null时需要加在第一页第一项之前)</summary>
        public Organization.Appliaction.ResponseModels.EvaluationItemDto? EvltAddedByMe { get; set; }
        /// <summary>用户评测点赞排名最高的信息</summary>
        public UseEvltLikeHighestInfo? UseEvltLikeHighestInfo { get; set; }
        /// <summary>活动(推广)码</summary>
        public string? Pcode { get; set; }
        /// <summary>活动数据,不为null</summary>
        public Organization.Appliaction.ResponseModels.ActivityDataDto ActivityData { get; set; } = default!;
    }

    /// <summary>
    /// 用户评测点赞排名最高的信息
    /// </summary>
    public class UseEvltLikeHighestInfo
    {
        /// <summary>评测id</summary>
        public Guid Id { get; set; }
        /// <summary>评测短id</summary>
        public string Id_s { get; set; } = default!;
        /// <summary>评论数</summary>
        public int CommentCount { get; set; }
        /// <summary>点赞数</summary>
        public int LikeCount { get; set; }
        /// <summary>点赞排名</summary>
        public int LikeRank { get; set; } = 99999;
        /// <summary>评测作者id</summary>
        public Guid UserId { get; set; }
        /// <summary>评测封面</summary>
        public string Cover { get; set; } = default!;
    }

#nullable disable
}
