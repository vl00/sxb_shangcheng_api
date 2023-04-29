using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{
    /// <summary>
    /// 审核成功命令
    /// </summary>
    public class AuditSuccessCommand : IRequest<bool>
    {
        /// <summary>
        /// 售后请求ID
        /// </summary>
        public Guid Id { get; set; }

        public Guid Auditor { get; set; }

        /// <summary>
        /// 实退金额
        /// </summary>
        public decimal? RefundAmount { get; set; }


        /// <summary>
        /// 特殊原因
        /// </summary>
        public OrderRefundSpecialReason SpecialReason { get; set; }

        /// <summary>
        /// 特殊原因备注
        /// </summary>
        public string SpecialReasonRemark { get; set; }


    }
}
