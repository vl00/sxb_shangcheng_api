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
    /// mini根据ids批量获取种草items
    /// </summary>
    public class MiniGetEvltsQueryArgs : IRequest<IEnumerable<MiniEvaluationItemDto>>
    {
        public IEnumerable<Guid> Ids { get; set; } = default!;
    }
}
#nullable disable
