using iSchool.Organization.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.Courses
{
    /// <summary>
    /// 商品的选项
    /// </summary>
    public class CGoodsProItemInfo
    {

        /// <summary>
        /// 商品Id
        /// </summary>
        public Guid GoodsId { get; set; }
                
        /// <summary>
        /// 选项名称
        /// </summary>
        public string ItemName { get; set; }
       
    }

    /// <summary>
    /// 商品的库存销量总量信息
    /// </summary>
    public class GoodsStockInfo 
    {
        /// <summary>
        /// 商品信息
        /// </summary>
        public CourseGoods Goods { get; set; }

        /// <summary>
        /// 商品的选项
        /// </summary>
        public List<CGoodsProItemInfo> Items { get; set; }
    }

}
