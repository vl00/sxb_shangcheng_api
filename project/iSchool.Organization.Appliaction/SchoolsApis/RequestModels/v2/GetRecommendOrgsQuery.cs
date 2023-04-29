using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// v2推荐机构(按pv排序)
    /// </summary>
    public class GetRecommendOrgsQuery : IRequest<GetRecommendOrgsQueryResult>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int Type { get; set; }
    }

#nullable disable
}
