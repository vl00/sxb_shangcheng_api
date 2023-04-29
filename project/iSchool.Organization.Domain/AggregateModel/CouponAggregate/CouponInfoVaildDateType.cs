using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    //1、按日期限定 2、按领券后期限 3、永久有效
    public enum CouponInfoVaildDateType
    {
        /// <summary>
        /// 指定日期
        /// </summary>
        SpecialDate = 1,
        /// <summary>
        /// 指定天数
        /// </summary>
        SpecialDays = 2,

        /// <summary>
        /// 永久的
        /// </summary>
        Forever = 3

    }
}
