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
    /// 退货用户寄回物流
    /// </summary>
    public class RefundUserSendbackKdwlCmd : IRequest<object>
    {
        /// <summary>退款单id</summary>
        public Guid Id { get; set; }

        /// <summary>快递公司编码</summary>
        public string Com { get; set; } = default!;
        /// <summary>快递单号</summary>
        public string Nu { get; set; } = default!;
    }

#nullable disable
}
