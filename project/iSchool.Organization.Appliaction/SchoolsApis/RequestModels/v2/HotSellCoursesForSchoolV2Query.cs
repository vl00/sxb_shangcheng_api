using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class HotSellCoursesForSchoolV2Query : IRequest<HotSellCoursesForSchoolV2QryResult>
    {
        public int MinAge { get; set; }
        public int MaxAge { get; set; }

        //public int Count { get; set; }
    }

    public class HotSellOrgsForSchoolV2Query : IRequest<HotSellOrgsForSchoolV2QryResult>
    {
        public int MinAge { get; set; }
        public int MaxAge { get; set; }

        //public int Count { get; set; }
    }
}

#nullable disable
