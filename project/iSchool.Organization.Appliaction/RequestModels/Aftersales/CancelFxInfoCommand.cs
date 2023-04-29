using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{
    /// <summary>
    /// 撤销收益
    /// </summary>
    public class CancelFxInfoCommand : IRequest
    {
        public string OrgOrderNo { get; set; }

        public Guid OrderDetailId { get; set; }

        public int RefundCount { get; set; }

        public Guid OrderUserId { get; set; }

    }
}
