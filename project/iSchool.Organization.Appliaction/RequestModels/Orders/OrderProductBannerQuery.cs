using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Orders
{
    public class OrderProductBannerQuery : IRequest<ResponseResult>
    {
        public Guid AdvanceOrderId { get; set; }
    }
}
