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
    /// mini 前端获取订单s的兑换码信息s （only read db）
    /// </summary>
    public class GetOrderRedeemInfoQueryArgs : IRequest<IEnumerable<OrderRedeemInfoDto>>
    {
        /// <summary>订单ids</summary>
        public Guid[] OrderIds { get; set; } = default!;
        
    }

#nullable disable
}
