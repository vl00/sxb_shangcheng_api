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
    /// 查询退款单物流快递详情
    /// </summary>
    public class GetRefundBillKdDetailQuery : IRequest<RefundBillKdDetailDto>
    {
        /// <summary>快递单</summary>
        public string Id { get; set; } = default!;

        public OrderRefunds? OrderRefund { get; set; }
    }

#nullable disable
}
