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
    /// 更新订单状态
    /// </summary>
    public class UpdateOrderStatusCommand : IRequest<bool>
    {                
        public Guid OrderId { get; set; }
        /// <summary>预订单id.(单个订单是为null)</summary>
        public Guid? AdvanceOrderId { get; set; }

        /// <summary>原来的状态</summary>
        public int? Status0 { get; set; }
        public int? Status0UnPaid_TimeoutMin { get; set; }

        /// <summary>新的状态</summary>
        public int NewStatus { get; set; }
        public DateTime? NewStatusOk_Paymenttime { get; set; }

    }

#nullable disable
}
