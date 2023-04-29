using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 评测详情
    /// </summary>
    public class EvltDetailQuery : IRequest<ResponseModels.EvltDetailDto>
    {
        public long No { get; set; }
        public Guid EvltId { get; set; }

        public bool AllowRecordPV { get; set; } = true;
    }
}
