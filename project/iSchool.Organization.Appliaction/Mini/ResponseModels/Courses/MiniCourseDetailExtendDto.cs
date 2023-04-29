using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class MiniCourseDetailExtendDto
    {

        /// <summary>
        /// 课程标签
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// 购前须知
        /// </summary>
        public IEnumerable<CourseNoticeItem> Notices { get; set; }


        /// <summary>
        /// 大家的种草(封面)
        /// </summary>
        public IEnumerable<EvaluationCover> Covers { get; set; }
    }

    public class CourseNoticeItem
    {
        /// <summary>标题</summary>
        public string Title { get; set; } = default!;
        /// <summary>内容</summary>
        public string Content { get; set; } = default!;
    }


    public class EvaluationCover
    {
        ///// <summary>评测图(原图)</summary>
        //public IEnumerable<string> Imgs { get; set; }
        ///// <summary>评测图(缩略图)</summary>
        //public IEnumerable<string> Imgs_s { get; set; }
        ///// <summary>视频地址</summary>
        //public string VideoUrl { get; set; }
        ///// <summary>视频封面图</summary>
        //public string VideoCoverUrl { get; set; }

        public string Cover { get; set; }
        /// <summary>
        /// 当前是否是图片   true 图片  false 视频
        /// </summary>
        public bool IsPicture { get; set; }
        /// <summary>评测id</summary>
        public Guid Id { get; set; }
        /// <summary>评测短id</summary>
        public string Id_s { get; set; } = default!;

    }

}
