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
    /// pc课程详情
    /// </summary>
    public class PcCourseDetailDto : CourseDetailsResponse, ISeoTDKInfo, IPageMeInfo
    {
        /// <summary>课程短id</summary>
        public new string Id_s { get; set; } = default!;
        /// <summary>课程名称.这个字段没用的,请用title字段</summary>
        public new string CName { get; set; } = default!;

        /// <summary>科目(中文)</summary>
        public new string SubjectDesc { get; set; } = default!;
        /// <summary>科目</summary>
        public new int Subject { get; set; }
        /// <summary>用于页面跳转的科目数值</summary>
        public int Subj { get; set; }

        /// <summary>科目s(中文)</summary>
        public new string[] SubjectDescs { get; set; } = default!;
        /// <summary>科目s</summary>
        public new int[] Subjects { get; set; } = default!;

        #region 关联机构信息
        /// <summary>机构短Id</summary>
        public new string? OrgNoId => OrgInfo?.Id_s;
        /// <summary>机构logo</summary>
        public new string? Logo => OrgInfo?.Logo;
        /// <summary>机构名称</summary>
        public new string OrgName => OrgInfo.Name;
        /// <summary>机构是否认证（true：认证；false：未认证）</summary>
        public new bool Authentication => OrgInfo?.Authentication ?? false;
        /// <summary>机构描述</summary>
        public new string? Desc => OrgInfo?.Desc;
        /// <summary>机构子描述</summary>
        public new string? SubDesc => OrgInfo?.Subdesc;
        #endregion

        /// <summary>字段已弃用</summary>
        public new object? EvaluationInfo { get; set; }

        /// <summary>机构信息</summary>
        public PcOrgItemDto OrgInfo { get; set; } = default!;

        /// <summary>相关评测</summary>
        public PcRelatedEvaluationsListDto RelatedEvaluations { get; set; } = default!;

        /// <summary>机构(相关)课程</summary>        
        public PcRelatedCoursesListDto RelatedCourses { get; set; } = new PcRelatedCoursesListDto();        

        public new string? Tdk_d => SeoTDKInfoUtil.GetTDK(this);
        public IUserInfo? Me { get; set; }
    }
}
#nullable disable
