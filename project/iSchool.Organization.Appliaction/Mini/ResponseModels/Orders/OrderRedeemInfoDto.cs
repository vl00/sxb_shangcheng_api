using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 订单de兑换码信息
    /// </summary>
    public class OrderRedeemInfoDto
    {
        /// <summary>订单id</summary>
        public Guid OrderId { get; set; }

        /// <summary>兑换码.</summary>
        public string RedeemCode { get; set; } = default!;
        /// <summary>兑换码链接url.</summary>
        public string Url { get; set; } = default!;
        /// <summary>兑换码提示框内容.</summary>
        public string Msg { get; set; } = default!;
        /// <summary>跳转兑换链接.</summary>
        public bool IsRedirect { get; set; }

        public RedeemCode Redeem0 { get; set; } = default!;
    }

}
