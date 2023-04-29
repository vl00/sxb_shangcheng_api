using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ViewModels.Supplier;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Supplier
{
    /// <summary>
    /// 供应商管理-列表
    /// </summary>
    public class SearchSupplierListQuery : IRequest<PagedList<SupplierItem>>
    {
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 供应商对公账号
        /// </summary>
        public string BankCardNo { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public List<Guid> OrganizationIds { get; set; }

        /// <summary>
        /// 是否私人
        /// </summary>
        public bool? IsPrivate { get; set; }

        /// <summary>
        /// 结算方式
        /// </summary>
        public int? BillingType { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
