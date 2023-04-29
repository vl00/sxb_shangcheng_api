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

    public class RefundCancelCmd : IRequest<object>
    {
        /// <summary>退款单id</summary>
        public Guid Id { get; set; }

        [JsonIgnore]
        public bool IsFromExpired { get; set; } = false;
    }

#nullable disable
}
