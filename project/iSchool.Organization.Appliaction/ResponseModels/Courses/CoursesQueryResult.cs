using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 学校-课程信息返回实体
    /// </summary>
    public class CoursesQueryResult
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid Id { get; set; }
       
        /// <summary>
        /// 课程短Id
        /// </summary>
        public string No { get; set; }        

        /// <summary>
        /// 产品名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程banner图片地址（json字符串）
        /// </summary>
        public string Banner { get; set; }

        /// <summary>
        /// 现在价格（认证则显示，否则不显示）
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 原始价格
        /// </summary>
        public decimal? OrigPrice { get; set; }

        /// <summary>
        /// 库存（认证则显示，否则不显示）
        /// </summary>
        public int? Stock { get; set; }

        /// <summary>
        /// 是否认证（true：认证；false：未认证）
        /// </summary>
        public bool Authentication { get; set; }
    }
}
