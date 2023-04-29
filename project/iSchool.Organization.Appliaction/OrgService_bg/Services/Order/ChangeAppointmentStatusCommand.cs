using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Order
{
    /// <summary>
    /// 更改约课状态
    /// </summary>
    public class ChangeAppointmentStatusCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid OrdId { get; set; }              

        /// <summary>
        /// 约课状态
        /// </summary>
        public int AppointmentStatus { get; set; }

        /// <summary>
        /// 操做者
        /// </summary>
        public Guid UserId { get; set; }
    }
}
