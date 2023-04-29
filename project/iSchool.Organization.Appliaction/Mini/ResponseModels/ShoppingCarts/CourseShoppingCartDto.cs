using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ViewModels.Coupon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// 购物车里的课程好物商品item dto
    /// </summary>
    public class CourseShoppingCartProdItemDto : CourseOrderProdItemDto
    {
        /// <summary>是否选中</summary>
        public bool Selected { get; set; }
        /// <summary>加入购物车时间(时间戳).</summary>
        [JsonConverter(typeof(DateTimeToTimestampJsonConverter))]
        public DateTime Time { get; set; }

        /// <summary>
        /// 前端传入的json对象`{}`额外数据.可null <br/>
        /// 例如 `{ fw: '', eid: '', surl: '' }`
        /// </summary>
        public JObject? Jo { get; set; }
    }

    public class CourseShoppingCartDto
    {
        public IList<CourseShoppingCartGroupItemDto> Items { get; set; } = default!;
    }

    public class CourseShoppingCartGroupItemDto
    {
        /// <summary>机构id</summary>
        public Guid OrgId { get; set; }
        /// <summary>机构name</summary>
        public string OrgName { get; set; } = default!;
        /// <summary>机构短id</summary>
        public string OrgId_s { get; set; } = default!;
        /// <summary>商品s</summary>
        public CourseShoppingCartProdItemDto[] Goods { get; set; } = default!;

        /// <summary>
        /// 是否拥有品牌券（指定品牌的券）
        /// </summary>
        public bool IsContainBrandCoupon { get; set; }

        /// <summary>
        /// 品牌凑单券
        /// </summary>
        public CouDanCoupon? CouDanCoupon { get; set; }

    }

#nullable disable
}
