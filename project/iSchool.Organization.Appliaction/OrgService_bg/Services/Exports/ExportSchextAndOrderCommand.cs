using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Exports
{
#nullable enable

    public class ExportSchextAndOrderCommand : IRequest<string>
    {        
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

#nullable disable
}
