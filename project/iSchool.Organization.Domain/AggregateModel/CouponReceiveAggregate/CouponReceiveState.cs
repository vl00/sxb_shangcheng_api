using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponReceiveAggregate
{
    public enum CouponReceiveState
    {

        /// <summary>
        /// 待使用
        /// </summary>
        [Description("待使用")]
        WaitUse = 1,
        /// <summary>
        /// 已使用
        /// </summary>
        [Description("已使用")]
        Used = 2,
        /// <summary>
        /// 预使用状态
        /// </summary>
        [Description("预使用状态")]
        PreUse = 4,

        /// <summary>
        /// 失效
        /// </summary>
        [Description("失效")]
        LoseEfficacy = 8
    }


    public enum CouponReceiveStateExt
    {
        /// <summary>
        /// 待使用
        /// </summary>
        [Description("待使用")]
        WaitUse = 1,
        /// <summary>
        /// 已使用
        /// </summary>
        [Description("已使用")]
        Used = 2,
        /// <summary>
        /// 预使用状态
        /// </summary>
        [Description("预使用状态")]
        PreUse = 4,

        /// <summary>
        /// 失效
        /// </summary>
        [Description("失效")]
        LoseEfficacy = 8,

        /// <summary>
        /// 过期
        /// </summary>
        [Description("已过期")]
        Expire = 16
    }
}
