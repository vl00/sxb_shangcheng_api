using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{

    public class AddOrderLogisticsCommand : IRequest<bool>
    {
        public Guid OrderId { get; set; }
        public Guid OrderDetailId { get; set; }

        public Guid UserId { get; set; }

        public List<OrderLogisticsData> OrderLogistics { get; set; }

    }

    public class OrderLogisticsData
    {
        public Guid? OrderLogisticsId { get; set; } = null;

        /// <summary>
        /// 快递公司编码
        /// </summary> 
        public string ExpressType { get; set; }

        /// <summary>
        /// 快递号
        /// </summary> 
        public string ExpressCode { get; set; }

        public short Number { get; set; }
    }


}
