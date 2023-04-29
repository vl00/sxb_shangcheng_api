using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    /// <summary>
    /// 优惠券类型：1、体验券 2、折扣券 3、满减券  4.立减券
    /// </summary>
    public enum CouponType
    {
        /// <summary>
        /// 体验券
        /// </summary>
        [Description("体验券")]
        TiYan = 1,
        /// <summary>
        /// 折扣券
        /// </summary>
        [Description("折扣券")]
        ZheKou = 2,
        /// <summary>
        /// 满减券
        /// </summary>
        [Description("满减券")]
        ManJian = 3,
        /// <summary>
        /// 立减券
        /// </summary>
        [Description("立减券")]
        LiJian = 4


    }
}
