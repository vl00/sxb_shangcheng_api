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
    /// 查找一个OrderDetail的退款数目
    /// 
    /// (成功数, 退款中数)
    /// </summary>
    public class OrderDetailRefundCountsQryArgs : IRequest<(int OkCount, int RefundingCount)>
    {
        public Guid OrderDetailId { get; set; }
    }

#nullable disable
}
