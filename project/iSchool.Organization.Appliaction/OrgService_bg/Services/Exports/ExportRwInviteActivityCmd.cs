using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Exports
{
#nullable enable

    public class ExportRwInviteActivityCmd : IRequest<string>
    {
        public int? CourseExchange_type { get; set; }
    }

#nullable disable
}
