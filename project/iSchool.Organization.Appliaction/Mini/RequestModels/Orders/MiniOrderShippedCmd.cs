using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// mini 确定收货
    /// </summary>
    public class MiniOrderShippedCmd : IRequest<bool>
    {
        /// <summary>订单Id</summary>
        public Guid OrderId { get; set; }
        /// <summary>是否自动确定收货</summary>
        public bool IsFromAuto { get; set; }
    }

    /// <summary>
    /// 确定收货ok后
    /// </summary>
    public class OrderShippedOkEvent : INotification
    {
        /// <summary>订单Id</summary>
        public Guid OrderId { get; set; }

        public bool IsFromAuto { get; set; }
    }

#nullable disable
}
