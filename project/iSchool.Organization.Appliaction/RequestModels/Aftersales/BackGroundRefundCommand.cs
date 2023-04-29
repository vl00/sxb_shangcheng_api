using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Aftersales
{
    public class BackGroundRefundCommand :IRequest<bool>
    {
        public Guid OrderDetailId { get; set; }

        public int RefundCount { get; set; }

        public Guid Auditor { get; set; }

    }
}
