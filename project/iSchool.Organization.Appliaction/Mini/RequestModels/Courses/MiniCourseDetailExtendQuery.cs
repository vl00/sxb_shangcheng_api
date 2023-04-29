using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;


namespace iSchool.Organization.Appliaction.RequestModels
{
    public class MiniCourseDetailExtendQuery : IRequest<MiniCourseDetailExtendDto>
    {
        /// <summary>
        /// 课程id
        /// </summary>
        public long No { get; set; }
    }
}
