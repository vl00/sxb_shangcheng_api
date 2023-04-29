using iSchool.Organization.Appliaction.ViewModels.Coupon;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{
    public class GoodsTypeQuery:IRequest<IEnumerable<GoodsTypeEnableRange>>
    {
        /// <summary>
        /// 1-课程 2-好物
        /// </summary>
        public int Type { get; set; }

    }
}
