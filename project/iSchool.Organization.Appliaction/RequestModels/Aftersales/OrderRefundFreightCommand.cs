using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{
    
    /// <summary>
    /// 退运费
    /// </summary>
    public class OrderRefundFreightCommand:IRequest
    {
        public Guid AdvanceOrderId { get; set; }
        public Guid OrderId { get; set; }

        public Guid OrderRefundId { get; set; }


        public decimal Freight { get; set; }
    }
}
