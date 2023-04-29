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
    /// 批量查询pc机构信息各统计数目
    /// </summary>
    public class PcGetOrgsCountsQuery : IRequest<PcGetOrgsCountsQueryResult>
    {
        public IEnumerable<Guid> OrgIds { get; set; } = default!;
    }
}
#nullable disable
