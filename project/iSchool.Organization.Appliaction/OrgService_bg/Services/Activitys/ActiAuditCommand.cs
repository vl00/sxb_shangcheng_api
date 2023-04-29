using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Activitys
{
#nullable enable

    /// <summary>
    /// 活动审核
    /// </summary>
    public class ActiAuditCommand : IRequest<ActiAuditCommandResult>
    {
        public Guid EvltId { get; set; }
        public Guid AebId { get; set; }
        public Guid AuditorId { get; set; }
        public bool IsPass { get; set; }
        public string? Adesc { get; set; }
        public byte? Areply { get; set; }
    }

    public class ActiAuditCommandResult
    {
        /// <summary>
        /// 0=正常<br/>
        /// 2=手机号冲突<br/>
        /// </summary>
        public int Errcode { get; set; }
        public string? Errmsg { get; set; }
    }

#nullable disable
}
