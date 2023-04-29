using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// pc机构详情
    /// </summary>
    public class PcOrgDetailQuery : IRequest<PcOrgDetailDto>
    {
        public long No { get; set; }
        public Guid OrgId { get; set; }
    }
}
#nullable disable
