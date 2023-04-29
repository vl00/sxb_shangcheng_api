using iSchool.Infrastructure.Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class MiniGrowUpDataDto
    {
        /// <summary>
        /// 孩子档案
        /// </summary>
        public List<MiniChildArchiveItemDto> ChildArchives { get; set; }

        /// <summary>
        /// 我的精选课程
        /// </summary>
        public List<(MiniCourseItemDto course, Guid OrderId)> MyCourses { get; set; }


        /// <summary>
        /// 我的种草
        /// </summary>
        public PagedList<MiniEvaluationItemDto> Evaluations { get; set; } = default!;


    }
}
