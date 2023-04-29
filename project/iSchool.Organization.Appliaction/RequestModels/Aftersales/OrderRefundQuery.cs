using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{
    public class OrderRefundQuery:IRequest<OrderRefunds>
    {
        public Guid OrderRefundId { get; set; }
    }
}
