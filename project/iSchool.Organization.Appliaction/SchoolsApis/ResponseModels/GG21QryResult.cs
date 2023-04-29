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
    public class GG21QryResult
    {
        public DateTime? Time { get; set; }

        /// <summary>(21个)课程s</summary>
        public IEnumerable<PcCourseItemDto2> Courses { get; set; } = default!;
    }
}

#nullable disable
