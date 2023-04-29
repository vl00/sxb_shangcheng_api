using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Activitys
{
#nullable enable

    public class AuditLsSpclQuery : IRequest<AuditLsSpclQueryResult>
    {
        public Guid ActivityId { get; set; }
    }

    public class AuditLsSpclQueryResult
    {
        public IEnumerable<(Guid Id, string Name)> Spcls { get; set; } = Enumerable.Empty<(Guid, string)>();
    }

#nullable disable
}
