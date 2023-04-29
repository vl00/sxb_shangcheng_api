using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class GetOrderMultipleKuaidisQuery : IRequest<(IEnumerable<OrderItemDto> OrderItems, string Qrcode)>
    {
        public Guid OrderId { get; set; }
    }
}
