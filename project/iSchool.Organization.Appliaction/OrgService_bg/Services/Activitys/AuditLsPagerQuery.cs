using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Activitys
{
#nullable enable

    public class BaseAuditLsPagerQuery
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int? Status { get; set; }
        public string? ActivityId { get; set; }
        public Guid[]? SpecialIds { get; set; }
        public string? UserName { get; set; }
        public string? EvltTitle { get; set; }
    }

    public class AuditLsPagerQuery : BaseAuditLsPagerQuery, IRequest<AuditLsPagerQueryResult>
    { }

    public class AuditLsPagerQueryResult
    {
        public PagedList<AuditLsPagerItemDto> PageInfo { get; set; } = default!;
    }

    public class AuditLsPagerItemDto
    {
        public Guid Id { get; set; }
        /// <summary>提交类型</summary>
        public int SubmitType { get; set; }
        /// <summary>审核状态</summary>
        public int AebStatus { get; set; }
        public DateTime? AuditTime { get; set; }
        public Guid? Auditor { get; set; }

        /// <summary>单篇奖金</summary>
        public decimal? PriceForOneEvaluation { get; set; }
        /// <summary>
        /// 额外奖金 <br/>
        /// 1个评测的作者的同手机号账号的第几篇
        /// </summary>
        public (int, IEnumerable<decimal>)? PriceForExtraBonus { get; set; }

        public Guid ActivityId { get; set; }
        public string? ActiTitle { get; set; }
        public bool A_isvalid { get; set; }

        public Guid EvaluationId { get; set; }
        public string Title { get; set; } = default!;
        public DateTime CreateTime { get; set; }
        public DateTime? Mtime { get; set; }
        public string? SpclTitle { get; set; }
        public bool S_isvalid { get; set; }

        public Guid UserId { get; set; }
        public string UserName { get; set; } = default!;
        public string? Mobile { get; set; }
        /// <summary>该手机号绑定账号数量</summary>
        public int UmacCount { get; set; }
        /// <summary>领取红包数量</summary>
        public int UGetRedpCount { get; set; }
    }

#nullable disable
}
