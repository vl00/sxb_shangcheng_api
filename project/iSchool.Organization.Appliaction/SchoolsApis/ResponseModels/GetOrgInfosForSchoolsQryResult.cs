using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class GetOrgInfosForSchoolsQryResult
    {
        /// <summary>机构/品牌s</summary>
        public IEnumerable<PcOrgItemDto> Orgs { get; set; } = default!;
    }

}

#nullable disable
