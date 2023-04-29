using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    ///获取单个兑换码
    /// </summary>
    public class QuerySingleRedeemCode : IRequest<RedeemCode>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid OrderId { get; set; }
    }
}
