using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Queries.Models
{
    /// <summary>
    /// 搜索优惠券指定的商品
    /// </summary>
    public class SearchCouponSpecialGoods
    {
        public string SearchText { get; set; }

        public Guid CouponId { get; set; }


    }
}
