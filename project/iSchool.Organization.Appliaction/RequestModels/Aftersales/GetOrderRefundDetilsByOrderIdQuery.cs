using iSchool.Organization.Appliaction.ResponseModels.Orders;
using iSchool.Organization.Appliaction.ViewModels.Aftersales;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{
    public class GetOrderRefundDetilsByOrderIdQuery:IRequest<List<OrderRefundDetail>>
    {
        public Guid OrderId { get; set; }
    }
}
