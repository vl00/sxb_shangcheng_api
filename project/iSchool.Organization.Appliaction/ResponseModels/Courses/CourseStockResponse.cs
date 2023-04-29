using iSchool.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    [Obsolete]
    public class CourseStockResponse
    {
        /// <summary>
        /// -3:库存未初始化
        /// <br/> -2:库存不足
        /// <br/> -1:不限库存
        /// <br/> 大于等于0:剩余库存（扣减之后剩余的库存）
        /// </summary>
        public int StockResult { get; set; }
        public int AddStockResult { get; set; }
        public int? GetStockResult { get; set; }

        public int? SyncSetResult { get; set; }

        public bool BgSetStockIsOk { get; set; }
    }

#nullable disable
}
