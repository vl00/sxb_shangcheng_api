using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 检查(待支付)订单是否过期
    /// </summary>
    public class CheckOrderIsExpiredCommand : IRequest<object>
    {                
        public Guid? AdvanceOrderId { get; set; }
    }

#nullable disable
}
