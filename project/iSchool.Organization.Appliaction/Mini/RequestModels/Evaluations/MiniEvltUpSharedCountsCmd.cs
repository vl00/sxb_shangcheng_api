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
    /// 更新种草分享数
    /// </summary>
    public class MiniEvltUpSharedCountsCmd : IRequest<bool>
    {
        public Guid EvltId { get; set; }
    }

#nullable disable
}
