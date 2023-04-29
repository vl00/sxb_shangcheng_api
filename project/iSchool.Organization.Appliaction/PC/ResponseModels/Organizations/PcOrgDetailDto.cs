using iSchool.Infrastructure;
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
    /// pc机构详情
    /// </summary>
    public class PcOrgDetailDto : IPageMeInfo
    {
        /// <summary>机构Id</summary>
        public Guid Id => OrgInfo.Id;
        /// <summary>机构短Id</summary>
        public string Id_s => OrgInfo?.Id_s!;
        /// <summary>机构名称</summary>
        public string Name => OrgInfo?.Name!;
        /// <summary>机构底图Url</summary>
        public string? OrgBaseMap { get; set; }
        /// <summary>机构Logo</summary>
        public string? Logo => OrgInfo?.Logo;
        /// <summary>是否认证（true：认证；false：未认证）</summary>
        public bool Authentication => OrgInfo?.Authentication ?? false;
        /// <summary>机构简介</summary>
        public string? Intro { get; set; }

        /// <summary>相关课程</summary>        
        public PcRelatedCoursesListDto RelatedCourses { get; set; } = default!;       

        /// <summary>相关评测推荐</summary>
        public PcRelatedEvaluationsListDto RecommendEvaluations { get; set; } = default!; // RelatedEvaluations

        /// <summary>机构信息</summary>
        public PcOrgItemDto OrgInfo { get; set; } = default!;

        public IUserInfo? Me { get; set; }

    }
}
#nullable disable
