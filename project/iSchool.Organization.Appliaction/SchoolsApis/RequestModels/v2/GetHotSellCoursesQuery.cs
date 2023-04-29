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
    /// v2热卖课程
    /// </summary>
    public class GetHotSellCoursesQuery : IRequest<GetHotSellCoursesQueryResult>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int MinAge { get; set; }
        public int MaxAge { get; set; }
    }

#nullable disable
}
