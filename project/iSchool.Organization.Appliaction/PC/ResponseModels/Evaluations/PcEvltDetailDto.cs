using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// pc评测详情
    /// </summary>
    public class PcEvltDetailDto : EvltDetailDto, ISeoTDKInfo, IPageMeInfo
    {
        /// <summary>用户(我)信息</summary>
        public IUserInfo? Me { get; set; }
        /// <summary>作者信息</summary>
        public PcEvltDetailDto_AuthorInfo AuthorInfo { get; set; } = default!;

        /// <summary>相关评测s</summary>
        public PcRelatedEvaluationsListDto RelatedEvaluations { get; set; } = default!;

        /// <summary>评论. 没内容为空数组.</summary>
        public new PagedList<PcEvaluationCommentDto> Comments { get; set; } = default!;
        ///// <summary>评论数</summary>
        //public new int CommentCount => Comments.TotalItemCount;

        public new string? Tdk_d => SeoTDKInfoUtil.GetTDK(this);
    }

    /// <summary>作者信息</summary>
    public class PcEvltDetailDto_AuthorInfo
    { 
        /// <summary>作者id</summary>
        public Guid Id { get; set; }
        /// <summary>作者名</summary>
        public string Name { get; set; } = default!;
        /// <summary>作者头像</summary>
        public string? HeadImg { get; set; }
        /// <summary>评测数</summary>
        public int EvaluationCount { get; set; }
        /// <summary>点赞数</summary>
        public int LikeCount { get; set; }
    }    
}
#nullable disable
