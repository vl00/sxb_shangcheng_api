using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 查询评测点赞
    /// </summary>
    public class EvltLikesQuery : IRequest<EvltLikesQueryResult>
    {
        public Guid[] EvltIds { get; set; } = default!;
    }

#nullable disable
}
