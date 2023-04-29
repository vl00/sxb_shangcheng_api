using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class MiniCourseItemDto
    {
        /// <summary>
        /// 课程id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 课程短id
        /// </summary>
        public string Id_s { get; set; }

        /// <summary>
        /// 是否认证（true：认证；false：未认证）
        /// </summary>
        public bool Authentication { get; set; }

        /// <summary>
        /// 课程标签
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 机构名称
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 机构Logo
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// 课程名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 课程banner图片地址
        /// </summary>
        public List<string> Banner { get; set; }
        /// <summary>
        /// 现在价格
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 原始价格
        /// </summary>
        public decimal? OrigPrice { get; set; }

        /// <summary>下架时间</summary> 
		public DateTime? LastOffShelfTime { get; set; }
    }
}
