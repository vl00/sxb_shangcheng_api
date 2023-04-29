using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Domain.AggregateModel.CouponAggregate
{
    public enum CouponInfoState
    {

        /// <summary>
        /// 下线
        /// </summary>
        Offline = 0,
        /// <summary>
        /// 上线
        /// </summary>
        Online = 1,
        /// <summary>
        /// 失效
        /// </summary>
        LoseEfficacy = 2,
    }
}
