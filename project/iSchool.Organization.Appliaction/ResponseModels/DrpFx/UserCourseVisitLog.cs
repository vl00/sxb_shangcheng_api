using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.DrpFx
{
    /// <summary>
    /// 用户最新访问课程浏览记录
    /// </summary>
    public class CourseVisitLogDetail
    {
        public string CourseName { get; set; }
        public decimal? CoursePrice { get; set; }
        public decimal? CourseOriginPrice { get; set; }
        public string CourseImgUrl { get; set; }
        public Guid CourseId { get; set; }
        public string CourseNo { get; set; }
        public DateTime AddTime { get; set; }
    }
    public class UserCourseVisitLog
    {
        public Guid UserId { get; set; }
        public List<CourseVisitLogDetail> VisitCourseLog { get; set; }


    }
}
