using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Activitys
{
#nullable enable

    public class ExportAuditLsCommand : BaseAuditLsPagerQuery, IRequest<string>
    {        
    }

#nullable disable
}
