using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Course
{
    public class CourseDetailsByIdQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 课程短Id
        /// </summary>
        public long No { get; set; }

        public bool AllowRecordPV { get; set; } = true;

    }

    

}
