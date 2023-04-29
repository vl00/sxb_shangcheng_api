using iSchool.Organization.Appliaction.ResponseModels.Orders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Orders
{
    public class OrgRelStatementDetailCommand : IRequest<StatementDetailResponseDto>
    {
        public Guid OrderDetailId { get; set; }
        public Guid UserId { get; set; }
    }
}
