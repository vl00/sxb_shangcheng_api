using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// mini 订单兑换码页面
    /// </summary>
    public class GetOrderRedeemDescQuery : IRequest<GetOrderRedeemDescQryResult?>
    {
        /// <summary>订单id or code</summary>
        public string OrderStr { get; set; } = default!;
        
    }

#nullable disable
}
