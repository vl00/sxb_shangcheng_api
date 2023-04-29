using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Orders
{
    /// <summary>
    /// 更新订单信息--机构反馈
    /// </summary>
    public class UpdateOrderInfoCommand : IRequest<bool>
    {
        /// <summary>
        /// 操作者
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid OrderId { get; set; }               

        /// <summary>
        /// 机构反馈
        /// </summary>
        public string SystemRemark { get; set; }
    }
}
