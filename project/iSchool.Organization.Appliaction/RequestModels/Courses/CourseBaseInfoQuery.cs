using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 课程详情 -- base info
    /// </summary>
    public class CourseBaseInfoQuery : IRequest<Domain.Course>
    {        
        public long No { get; set; }
        public Guid CourseId { get; set; }

        public bool AllowNotValid { get; set; } = false;
    }
}
