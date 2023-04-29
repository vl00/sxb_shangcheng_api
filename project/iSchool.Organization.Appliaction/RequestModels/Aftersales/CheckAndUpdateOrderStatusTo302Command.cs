using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{
    /// <summary>
    /// 302状态是待收货状态，它只能从303中流转过来。
    /// </summary>
    public class CheckAndUpdateOrderStatusTo302Command:IRequest
    {
        public Guid OrderId { get; set; }

        public Guid OrderDetailId { get; set; }

    }
}
