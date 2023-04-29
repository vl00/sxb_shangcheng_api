using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{

   /// <summary>
   /// 统计订单详情申请退款审核中的退款数量
   /// </summary>
    public class StaticApplyRefundAuditingCountCommand : IRequest<int>
    {

        public Guid OrderDetailId { get; set; }
    }
}
