using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using static iSchool.Organization.Appliaction.Service.Aftersales.CheckAndUpdateOrderToRefundSuccessCommandHandler;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{

    /// <summary>
    /// 检查并更新订单是否能转为退款状体。
    /// </summary>
    public class CheckAndUpdateOrderToRefundSuccessCommand : IRequest<OrderRefundSuccessStatusChangeType>
    {


        public Guid OrderId { get; set; }

        public Guid OrderDetailId { get; set; }


        public CheckAndUpdateOrderToRefundSuccessCommand(Guid orderId,Guid orderDetailId)
        {

            this.OrderId = orderId;
            this.OrderDetailId = orderDetailId;

        }
    }
}
