using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels
{
    /// <summary>
    /// 评测用于展示已有课程专用实体
    /// </summary>
    public class ShowDBCourseModel
    {
        /// <summary>
        /// 上课时长
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// 上课方式
        /// </summary>
        public string Modes { get; set; }

        /// <summary>
        /// 科目分类
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 年龄段
        /// </summary>
        public string AgeRange { get; set; }
    }
}
