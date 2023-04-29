using iSchool.Organization.Appliaction.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class MiniIndexDataDto
    {

        /// <summary>
        /// 精选课程
        /// </summary>
        public List<MiniCourseItemDto> Courses { get; set; }

    }
}
