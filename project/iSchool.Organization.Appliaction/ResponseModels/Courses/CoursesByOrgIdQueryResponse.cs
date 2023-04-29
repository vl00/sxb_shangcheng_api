using iSchool.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Infrastructure;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 根据机构Id,获取课程列表的返回实体类
    /// </summary>
    public class CoursesByOrgIdQueryResponse
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public PageInfoResult PageInfo { get; set; }

        /// <summary>
        /// 课程列表
        /// </summary>
        public List<CoursesData> CoursesDatas { get; set; }
    }

    /// <summary>
    /// 课程信息DB
    /// </summary>
    public class CoursesData
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid Id { get; set; }

        //private string no; 

        /// <summary>
        /// 课程短Id
        /// </summary>
        public string No { get; set; }
        //{
        //    get { return no; }
        //    set { try { no = UrlShortIdUtil.Long2Base32(Convert.ToInt64(value)); } catch { no = value; }  }

        //}

        /// <summary>
        /// 产品名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程banner图片地址
        /// </summary>
        public List<string> Banner { get; set; } = new List<string>();

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
        /// <summary>
        /// 课程标签
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// 下架时间
        /// </summary>
        public string LastOffShelfTime { get; set; }
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
        public  bool LimitedTimeOffer { get; set; }


    }

    /// <summary>
    /// 课程信息DB
    /// </summary>
    public class CoursesDataDB
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid Id { get; set; }

        //private string no;

        /// <summary>
        /// 课程短Id
        /// </summary>
        public string No { get; set; }
        //{
        //    get { return no; }
        //    set { try { no = UrlShortIdUtil.Long2Base32(Convert.ToInt64(value)); } catch { no = value; } }

        //}

        /// <summary>
        /// 课程名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 课程banner图片地址
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
        /// <summary>
        /// 年龄段起始
        /// </summary>
        public int? Minage { get; set; }
        /// <summary>
        /// 年龄段结束
        /// </summary>
        public int? Maxage { get; set; }
        /// <summary>
        /// 科目
        /// </summary>
        public int? Subject { get; set; }
        /// <summary>
        /// 源价
        /// </summary>
        public decimal Origprice { get; set; }
        /// <summary>
        /// 下架时间
        /// </summary>
        public DateTime LastOffShelfTime { get; set; }
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
        /// <summary>
        /// 年龄段
        /// </summary>
        public int Age { get; set; }
        /// <summary>
        /// 商品分类
        /// </summary>
        public string CommodityTypes  { get; set; }
    }

}
