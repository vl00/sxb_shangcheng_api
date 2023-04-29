using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Mini.RequestModels.Orders
{
   public  class OrderPresentedPointsQuery:IRequest<int?>
    {
        public Guid OrderDetailId { get; set; }
    }
}
