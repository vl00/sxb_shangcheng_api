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
    /// 冻结金额内部接口调用直接入账
    /// </summary>
    public class WalletFreezeAmountIncomeApiArgs : IRequest<WalletFreezeAmountIncomeApiResult>
    {
        /// <summary>用户id</summary>
        public Guid UserId { get; set; }
        /// <summary>冻结变动金额（正数）</summary>
        public decimal BlockedAmount { get; set; }
        /// <summary>备注</summary>
        public string? Remark { get; set; }
        /// <summary>订单id</summary>
        public Guid OrderId { get; set; }
        /// <summary>0 = SignIn(签到), 1 = OrgGoodNewReward(机构新人返现)</summary>
        public int Type { get; set; }

        public object? _others { get; set; }
    }

#nullable disable
}

