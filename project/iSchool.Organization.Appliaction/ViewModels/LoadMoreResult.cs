using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction
{
    public class LoadMoreResult<T>
    {
        /// <summary>
        /// 数据项
        /// </summary>
        public IEnumerable<T> CurrItems { get; set; }
        /// <summary>
        /// 当前页码
        /// </summary>
        public int CurrPageIndex { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPageCount { get; set; }
        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }
    }
}
