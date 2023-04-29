using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 分页信息
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; } = 20;

        ///// <summary>
        ///// 总记录数
        ///// </summary>
        //public int TotalCount { get; set; }
    }

    /// <summary>
    /// 分页信息
    /// </summary>
    public class PageInfoOfResponse
    {

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; } = 20;

    }
}
