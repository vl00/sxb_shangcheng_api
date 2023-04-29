using iSchool.Organization.Appliaction.ViewModels.Supplier;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Services.Supplier
{
    public class SupplierInfoByIdQuery : IRequest<SupplierInfo>
    {
        public Guid Id { get; set; }
    }
}
