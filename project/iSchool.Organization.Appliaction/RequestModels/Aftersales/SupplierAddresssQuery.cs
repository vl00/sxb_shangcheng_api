using iSchool.Organization.Appliaction.ViewModels.Aftersales;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{
    public class SupplierAddresssQuery:IRequest<Address>
    {
        public Guid OrderRefundId { get; set; }
    }
}
