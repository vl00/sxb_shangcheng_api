using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    /// <summary>
    ///课程列表筛选排序
    /// </summary>
    public enum CourseFilterSortType
    {
        /// <summary>
        /// 综合排序
        /// </summary>
        [Description("综合排序")]
        Default = 1,
        /// <summary>
        /// 最新上架降序
        /// </summary>
        [Description("最新上架降序")]
        New = 2,
      
        /// <summary>
        /// 价格从低到高
        /// </summary>
        [Description("价格从低到高")]
        PriceLowToHigh = 3,

        /// <summary>
        /// 价格从高到低
        /// </summary>
        [Description("价格从高到低")]
        PriceHighToLow = 4,
        /// <summary>
        /// 销量排序降序
        /// </summary>
        [Description("销量排序降序")]
        SaleVolume = 5,
        /// <summary>
        /// 销量排序升序
        /// </summary>
        [Description("销量排序升序")]
        SaleVolumeAsc = 6,
        /// <summary>
        /// 种草数量升序
        /// </summary>
        [Description("种草数量升序")]
        GrassCountAsc = 7,
        /// <summary>
        /// 种草数量降序
        /// </summary>
        [Description("种草数量降序")]
        GrassCountDesc = 8,
        /// <summary>
        /// 最新上架升序
        /// </summary>
        [Description("最新上架升序")]
        NewAsc = 9
    }
    public enum CourseFilterCutomizeType
    {
        /// <summary>
        /// 默认
        /// </summary>
        [Description("默认")]
        Default = 1,
        /// <summary>
        /// 官方认证
        /// </summary>
        [Description("官方认证")]
        OfficialAuth = 2,
        /// <summary>
        /// 0元学
        /// </summary>
        [Description("0元学")]
        Free = 3,

        /// <summary>
        /// 低价体验课
        /// </summary>
        [Description("低价体验课")]
        LowPriceExpirence = 4,

        /// <summary>
        /// 系统课
        /// </summary>
        [Description("系统课")]
        SystemCourse = 5,

    }


    public enum CourseFilterCutomizeTypeV1
    {
        /// <summary>
        /// 限时闪购
        /// </summary>
        [Description("限时闪购")]
        LimitTime = 1,
        /// <summary>
        /// 热销榜单
        /// </summary>
        [Description("热销榜单")]
        HotRank = 2,
        /// <summary>
        /// 0元学
        /// </新人专享>
        [Description("新人专享")]
        ForNew = 3,



    }
}
