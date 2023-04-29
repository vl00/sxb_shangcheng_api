using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class OrderLogisticsByOrderIdQuery : IRequest<OrderLogisticsDto>
    {
        /// <summary>
        /// 详情id
        /// </summary>
        public Guid OrderDetailId { get; set; }
    }
}
