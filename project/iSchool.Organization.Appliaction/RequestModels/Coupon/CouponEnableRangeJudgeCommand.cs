using iSchool.Organization.Appliaction.Queries;
using iSchool.Organization.Domain.AggregateModel.CouponAggregate;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Coupon
{
    /// <summary>
    /// 裁判券的可用范围
    /// </summary>
    public class CouponEnableRangeJudgeCommand : IRequest<IEnumerable<SKUInfo>>
    {

        public IEnumerable<EnableRange> EnableRanges { get; set; }
        public IEnumerable<Guid> SKUIds { get; set; }

    }
}
