using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    //1.指定商品 2.指定商品类型 3.指定品牌 4.
    public enum CouponEnableRangeType
    {
        /// <summary>
        /// 指定商品
        /// </summary>
        SpecialGoods = 1,
        /// <summary>
        /// 指定商品类型
        /// </summary>
        SpecialGoodsType = 2,
        /// <summary>
        /// 指定品牌
        /// </summary>
        SpcialBrand = 3,
        /// <summary>
        /// 全平台商品
        /// </summary>
        Alls = 4,

    }
}
