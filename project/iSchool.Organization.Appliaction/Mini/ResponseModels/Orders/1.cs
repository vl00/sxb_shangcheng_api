using iSchool.Organization.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class CheckGoodSkuCmdResult
    {
        /// <summary>
        /// 有效的商品s.为null时可能代表检测有错.
        /// </summary>
        public List<ApiCourseGoodsSimpleInfoDto> CourseGoodsLs { get; set; } = default!;

        /// <summary>无效(下架)的商品s</summary>
        public List<ApiCourseGoodsSimpleInfoDto>? NotValids { get; set; }
        /// <summary>无库存的商品s</summary>
        public List<ApiCourseGoodsSimpleInfoDto>? NoStocks { get; set; }
        /// <summary>价格变动的商品s</summary>
        public List<ApiCourseGoodsSimpleInfoDto>? PriceChangeds { get; set; }

    }

#nullable disable
}
