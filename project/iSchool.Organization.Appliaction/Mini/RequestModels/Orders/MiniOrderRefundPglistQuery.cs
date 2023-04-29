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
    /// mp 订单退款list
    /// </summary>
    public class MiniOrderRefundPglistQuery : IRequest<MiniOrderPglistQryResult>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        ///// <summary>订单状态</summary>
        //public int Status { get; set; } = 0;
        /// <summary>用户id</summary>
        public Guid? UserId { get; set; }
    }

#nullable disable
}
