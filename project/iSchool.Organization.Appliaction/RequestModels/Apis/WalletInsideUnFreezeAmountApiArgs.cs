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
    /// 解冻结金额内部接口调用直接入账
    /// </summary>
    public class WalletInsideUnFreezeAmountApiArgs : IRequest<WalletInsideUnFreezeAmountApiResult>
    {
        /// <summary>冻结id</summary>
        public string FreezeMoneyInLogId { get; set; } = default!;
        /// <summary>订单id</summary>
        public Guid OrderId { get; set; } = default;
        /// <summary>0 = SignIn(签到), 1 = OrgGoodNewReward(机构新人返现)</summary>
        public int Type { get; set; }

        public string? Remark { get; set; }

        public object? _others { get; set; }
    }

#nullable disable
}

