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
    /// mini 订单详情
    /// </summary>
    public class MiniOrderDetailQuery : IRequest<MiniOrderDetailQryResult>
    {
        public Guid OrderId { get; set; }
        public string? OrderNo { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public Guid UserId { get; set; }
    }

#nullable disable
}
