using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{

   /// <summary>
   /// 统计订单详情申请退款审核的总数量（包含审核通过已经审核中的，不包含审核失败以及取消申请的）
   /// </summary>
    public class StaticApplyRefundAuditCountCommand : IRequest<int>
    {

        public Guid OrderDetailId { get; set; }
    }
}
