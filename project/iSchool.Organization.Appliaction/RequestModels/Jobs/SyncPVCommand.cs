using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 同步PV
    /// </summary>
    public class SyncPVCommand : IRequest
    {
        public DateTime? Time { get; set; }
    }

    public class SyncOrgPvCommand : IRequest
    {
        public DateTime? Time { get; set; }
    }
}
