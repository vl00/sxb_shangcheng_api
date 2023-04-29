using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    public class GetRefundDetailQuery : IRequest<RefundDetailDto>
    {

        public string Id { get; set; } = default!;
    }

#nullable disable
}
