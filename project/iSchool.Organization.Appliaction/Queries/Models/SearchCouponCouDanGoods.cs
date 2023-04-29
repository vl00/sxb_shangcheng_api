using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Queries.Models
{

    /// <summary>
    /// 搜索优惠券凑单商品
    /// </summary>
    public class SearchCouponCouDanGoods
    {
        public string SearchText { get; set; }

        public Guid CouponId { get; set; }

        public List<Guid> CheckedSkuIds { get; set; }

    }
}
