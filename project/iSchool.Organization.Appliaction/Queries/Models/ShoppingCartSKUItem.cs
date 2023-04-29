using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Queries.Models
{
    /// <summary>
    /// 购物车里SKU的选中项
    /// </summary>
    public class ShoppingCartSKUItem
    {

        public Guid SKUId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal UnitPrice { get; set; }

        public Guid BrandId { get; set; }

        public List<int> GoodsTypeIds { get; set; }


    }
}
