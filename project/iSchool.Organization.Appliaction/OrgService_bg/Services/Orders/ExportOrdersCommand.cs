using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Orders
{
#nullable enable

    public class ExportOrdersCommand : IRequest<string>
    {        
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

#nullable disable
}
