using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// pc课程详情
    /// </summary>
    public class PcCourseDetailQuery : IRequest<PcCourseDetailDto>
    {
        public long No { get; set; }
        public Guid CourseId { get; set; }

        public bool AllowRecordPV { get; set; } = true;
    }
}
#nullable disable
