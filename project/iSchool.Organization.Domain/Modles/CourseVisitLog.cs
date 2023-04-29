using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.Modles
{
    public class CourseVisitLog
    {
        public Guid CourseId { get; set; }
        public DateTime AddTime { get; set; }
    }
}
