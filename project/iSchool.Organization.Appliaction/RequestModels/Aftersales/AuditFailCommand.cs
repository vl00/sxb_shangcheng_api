using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{
    /// <summary>
    /// 审核失败命令
    /// </summary>
    public class AuditFailCommand:IRequest<bool>
    {
        /// <summary>
        /// 售后请求ID
        /// </summary>
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        [Required]
        public string Reason { get; set; }


        public Guid Auditor { get; set; }
    }
}
