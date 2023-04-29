using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 品牌s的商品数s
    /// </summary>
    public class MiniGetOrgsGoodsCountsQryResult
    {
        /// <summary>
        /// { "orgId": count }
        /// </summary>
        public Dictionary<Guid, int> Dict { get; set; } = default!;
    }

}
