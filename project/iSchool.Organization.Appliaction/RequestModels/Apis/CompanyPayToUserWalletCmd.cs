using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 调用秀彬api[公司打款入账个人]
    /// </summary>
    public class CompanyPayToUserWalletCmd : IRequest<JToken?>
    { 
        /// <summary>给谁</summary>
        public Guid ToUserId { get; set; }
        /// <summary>支付金额</summary>
        public decimal Money { get; set; }

        public Guid OrderId { get; set; }

        public string Remark { get; set; } = default!;

        public object? _others { get; set; }
    }

#nullable disable
}

