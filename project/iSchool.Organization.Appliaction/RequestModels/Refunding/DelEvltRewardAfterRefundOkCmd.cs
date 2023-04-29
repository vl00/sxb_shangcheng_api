using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 退款成功后删除种草机会
    /// </summary>
    public class DelEvltRewardAfterRefundOkCmd : IRequest
    {
        /// <summary>退款单id</summary>
        public Guid Id { get; set; }
        public OrderRefunds? OrderRefund { get; set; } = default!;
    }

#nullable disable
}
