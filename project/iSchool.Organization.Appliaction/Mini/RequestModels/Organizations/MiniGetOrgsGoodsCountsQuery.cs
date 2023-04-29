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
    /// get 品牌s的商品数s
    /// </summary>
    public class MiniGetOrgsGoodsCountsQuery : IRequest<MiniGetOrgsGoodsCountsQryResult>
    {
        public Guid[] OrgIds { get; set; } = default!;
    }

#nullable disable
}
