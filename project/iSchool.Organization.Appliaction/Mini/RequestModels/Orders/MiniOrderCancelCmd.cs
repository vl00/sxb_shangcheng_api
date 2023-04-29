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
    /// <summary>
    /// mini 订单取消
    /// </summary>
    public class MiniOrderCancelCmd : IRequest<bool>
    {
        /// <summary>
        /// (预)订单ID
        /// </summary>
        public Guid OrderId { get; set; }

        [JsonIgnore]
        public bool IsFromExpired { get; set; } = false;
    }


#nullable disable
}
