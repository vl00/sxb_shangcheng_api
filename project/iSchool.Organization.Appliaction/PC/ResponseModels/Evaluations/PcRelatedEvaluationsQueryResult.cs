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
    //public class PcRelatedEvaluationsQueryResult
    //{
    //    /// <summary>相关评测s</summary>
    //    public IEnumerable<EvaluationItemDto> Items { get; set; } = default!;
    //}

    /// <summary>相关评测s</summary>
    public class PcRelatedEvaluationsListDto
    {
        /// <summary>机构短id,用于跳转</summary>
        public string? OrgId { get; set; }
        /// <summary>
        /// 用于页面跳转的科目数值<br/>
        /// 有机构短id时此数值为null<br/>
        /// 数值为null或0 首页科目栏选中全部
        /// </summary>
        public int? Subj { get; set; }
        /// <summary>相关评测s</summary>
        public IEnumerable<EvaluationItemDto> Evaluations { get; set; } = default!;
    }
}
#nullable disable
