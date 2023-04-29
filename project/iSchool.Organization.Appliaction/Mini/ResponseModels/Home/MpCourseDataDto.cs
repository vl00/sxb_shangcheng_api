using iSchool.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class MpCourseDataDto
    {
        /// <summary>课程Id</summary>
        public Guid Id { get; set; }

        /// <summary>课程短Id</summary>
        public string Id_s { get; set; }

        ///// <summary>
        ///// 产品名称
        ///// </summary>
        //public string Name { get; set; }

        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程banner图片地址
        /// </summary>
        public List<string> Banner { get; set; } = new List<string>();

        /// <summary>
        /// 现在价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 原始价格
        /// </summary>
        public decimal? OrigPrice { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int? Stock { get; set; }

        /// <summary>
        /// 是否认证（true：认证；false：未认证）
        /// </summary>
        public bool Authentication { get; set; }

        /// <summary>
        /// 课程标签
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// 下架时间
        /// </summary>
        [JsonConverter(typeof(DateTimeToTimestampJsonConverter))]
        public DateTime? LastOffShelfTime { get; set; }
        /// <summary>
        /// 是否新人立返
        /// </summary>
        public bool CanNewUserReward { get; set; }
        /// <summary>
        /// 是否新人专享
        /// </summary>
        public bool NewUserExclusive { get; set; }
        /// <summary>
        /// 是否限时优惠     
        /// </summary>
        public bool LimitedTimeOffer { get; set; }


    }
}
