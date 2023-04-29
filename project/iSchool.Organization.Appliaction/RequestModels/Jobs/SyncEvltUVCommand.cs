using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 同步评测UV
    /// </summary>
    public class SyncEvltUVCommand : IRequest
    {
        public DateTime? Time { get; set; }
    }
}
