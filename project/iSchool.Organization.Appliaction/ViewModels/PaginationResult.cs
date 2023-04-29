using System.Collections.Generic;
using System.Linq;

namespace iSchool.Organization.Appliaction
{
    public class PaginationResult<T>
    {
        /// <summary>
        /// 数据项
        /// </summary>
        public IEnumerable<T> Data { get; set; }
        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public long Total { get; set; }

        public static PaginationResult<T> Default(int pageIndex, int pageSize)
        {
            return new PaginationResult<T>() {
                Data = Enumerable.Empty<T>(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                Total = 0
            };
        }
    }
}
