using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Activitys
{
#nullable enable

    public class AuditLsActiQuery : IRequest<AuditLsActiQueryResult>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int? Type { get; set; }
    }

    public class AuditLsActiQueryResult
    {
        public PagedList<Activity> Activitys { get; set; } = default!;
    }

#nullable disable
}
