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
    public class PcOrgIndexQueryResult : IPageMeInfo
    {
        /// <summary>分页信息</summary>
        public PagedList<PcOrgItemDto> PageInfo { get; set; } = default!;
        /// <summary>用户(我)信息</summary>
        public IUserInfo? Me { get; set; }
        /// <summary>机构分类科目栏目</summary>
        public IEnumerable<SelectItemsKeyValues>? AllOrgTypes { get; set; }
    }

    /// <summary>
    /// pc机构列表项
    /// </summary>
    public class PcOrgItemDto
    {
        /// <summary>机构id</summary>
        public Guid Id { get; set; }
        /// <summary>机构短id</summary>
        public string Id_s { get; set; } = default!;
        /// <summary>机构名</summary>
        public string Name { get; set; } = default!;
        /// <summary>机构logo</summary>
        public string? Logo { get; set; }
        /// <summary>是否已认证</summary>
        public bool Authentication { get; set; }
        /// <summary>描述</summary> 
        public string? Desc { get; set; }
        /// <summary>子描述</summary> 
        public string? Subdesc { get; set; }

        /// <summary>课程数量</summary> 
        public int CourceCount { get; set; } = -1;
        /// <summary>评测数量</summary> 
        public int EvaluationCount { get; set; } = -1;
        /// <summary>商品数量</summary> 
        public int GoodsCount { get; set; } = -1;

        /// <summary>pc的url</summary>
        public string? PcUrl { get; set; }
        /// <summary>m站的url</summary>
        public string? MUrl { get; set; }
        /// <summary>微信小程序二维码</summary>
        public string? MpQrcode { get; set; }
        /// <summary>h5跳微信小程序url</summary>
        public string? H5ToMpUrl { get; set; }

        /// <summary>true=有效 false=无效</summary>
        public bool IsOnline { get; set; } = true;
    }

    /// <summary>
    /// pc机构列表项
    /// </summary>
    public class PcOrgItemDto0
    {
        /// <summary>机构id</summary>
        public Guid Id { get; set; }
        /// <summary>机构短id</summary>
        public string Id_s { get; set; } = default!;
        /// <summary>机构名</summary>
        public string Name { get; set; } = default!;
        /// <summary>机构logo</summary>
        public string? Logo { get; set; }
        /// <summary>是否已认证</summary>
        public bool Authentication { get; set; }
        /// <summary>描述</summary> 
        public string? Desc { get; set; }
        /// <summary>子描述</summary> 
        public string? Subdesc { get; set; }
    }
}
#nullable disable
