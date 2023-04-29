using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 获取种草分享数s
    /// </summary>
    public class GetEvltMiniSharedCountsQueryArgs : IRequest<IEnumerable<(Guid EvltId, int SharedCount)>>
    {
        public GetEvltMiniSharedCountsQueryArgs(Guid evltId) => this.EvltIds = new Guid[] { evltId };

        public GetEvltMiniSharedCountsQueryArgs(Guid[] evltIds) => this.EvltIds = evltIds ?? new Guid[0];

        public Guid[] EvltIds { get; set; } = default!;
    }

#nullable disable
}
