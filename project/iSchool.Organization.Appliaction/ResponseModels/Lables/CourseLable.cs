using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.Lables
{
    /// <summary>
    /// 课程卡片实体
    /// </summary>
    public class CourseLable
    {
        /// <summary>
        /// 课程长Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 课程短Id
        /// </summary>
        public string Id_s { get; set; }

        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程价格
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 课程封面图
        /// </summary>
        public string CoverUrl { get; set; }
    }
    /// <summary>
    /// 课程卡片返回实体
    /// </summary>
    public class CoursesLablesResponse 
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfoResult PageInfo { get; set; }

        /// <summary>
        /// 课程卡片列表
        /// </summary>
        public List<CourseLable> ListLables { get; set; }
    }

}
