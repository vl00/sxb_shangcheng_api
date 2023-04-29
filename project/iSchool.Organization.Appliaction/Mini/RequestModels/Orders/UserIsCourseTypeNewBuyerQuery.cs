using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 用户是否新用户（相对于course type来说）
    /// </summary>
    public class UserIsCourseTypeNewBuyerQuery : IRequest<UserIsCourseTypeNewBuyerQryResult>
    {
        /// <summary>用户Id</summary>
        public Guid UserId { get; set; }

        /// <summary>orderid not in</summary>
        public Guid[]? ExcludedOrderIds { get; set; }

        public CourseTypeEnum CourseType { get; set; }

        ///// <summary>是否也把待支付的单也计算在内</summary>
        //public bool AllowUnpaid { get; set; } = false;
    }

#nullable disable
}
