using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 分页信息
    /// </summary>
    public class PageInfoResult
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPage { get; set; } = 0;

        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalCount { get; set; } = 0;
    }
}
