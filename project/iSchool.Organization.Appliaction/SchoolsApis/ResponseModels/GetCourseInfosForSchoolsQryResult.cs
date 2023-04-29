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
    public class GetCourseInfosForSchoolsQryResult
    {
        /// <summary>课程s</summary>
        public IEnumerable<PcCourseItemDto3> Courses { get; set; } = default!;
        /// <summary>错误的短ids</summary>
        public List<string> ErrSids { get; set; } = default!;
        /// <summary>错误的ids</summary>
        public List<Guid> ErrIds { get; set; } = default!;
    }

}

#nullable disable
