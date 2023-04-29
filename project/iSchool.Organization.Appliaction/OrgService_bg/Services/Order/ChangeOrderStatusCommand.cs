using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Order
{
    /// <summary>
    /// 更改订单状态
    /// </summary>
    public class ChangeOrderStatusCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// 操做者
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int Status { get; set; }
    }
}
