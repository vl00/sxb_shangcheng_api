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
    /// 检查订单状态(未支付)然后try重新支付
    /// </summary>
    public class TryRePayorderCommand : IRequest<bool>
    {                
        /// <summary>预订单id.</summary>
        public Guid? AdvanceOrderId { get; set; }
        public OrderDetailSimQryResult? OrdersEntity { get; set; }

    }

#nullable disable
}
