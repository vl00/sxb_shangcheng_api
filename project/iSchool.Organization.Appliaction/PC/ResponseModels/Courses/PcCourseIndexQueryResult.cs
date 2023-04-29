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
    public class PcCourseIndexQueryResult : IPageMeInfo
    {
        /// <summary>分页信息</summary>
        public PagedList<PcCourseItemDto> PageInfo { get; set; } = default!;
        /// <summary>用户(我)信息</summary>
        public IUserInfo? Me { get; set; }
        /// <summary>课程列表科目栏目</summary>
        public IEnumerable<SelectItemsKeyValues>? Subjs { get; set; }
        /// <summary>机构信息</summary>
        public PcOrgItemDto? OrgInfo { get; set; }
    }

    /// <summary>
    /// pc课程列表项
    /// </summary>
    public class PcCourseItemDto : CoursesData
    {
        /// <summary>该字段已弃用,请使用'Id_s'字段</summary>
        public new long? No { get; set; }
        /// <summary>课程短id</summary>
        public string Id_s { get; set; } = default!;
        /// <summary>机构名</summary>
        public new string? Name { get; set; }
        /// <summary>课程标题</summary>
        public new string? Title { get; set; }
        /// <summary>课程副标题</summary>
        public string? Subtitle { get; set; }
        /// <summary>课程科目</summary>
        public int Subject { get; set; }
    }
}
#nullable disable
