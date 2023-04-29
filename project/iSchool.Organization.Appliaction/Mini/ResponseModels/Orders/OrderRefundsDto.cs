using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class OrderRefundsDto
    {
        public Guid Id { get; set; }

        public Guid? OrderId { get; set; }

        public Guid? OrderDetailId { get; set; }

        public Guid? ProductId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public short Count { get; set; }
        /// <summary>
        /// 1.退款  2.换货 3.极速退款  4.后台退款
        /// </summary>
        public byte? Type { get; set; }

        public byte? Status { get; set; }
    }
}
