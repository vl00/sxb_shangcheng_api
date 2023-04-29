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
    /// pc评测详情
    /// </summary>
    public class PcEvltDetailQuery : IRequest<PcEvltDetailDto>
    {
        public long No { get; set; }
        public Guid EvltId { get; set; }
    }
}
#nullable disable
