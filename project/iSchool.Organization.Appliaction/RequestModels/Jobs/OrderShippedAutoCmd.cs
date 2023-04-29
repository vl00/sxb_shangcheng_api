using MediatR;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 用于后台服务自动确定收货
    /// </summary>
    public class OrderShippedAutoCmd : IRequest<OrderShippedAutoCmdResult>
    {
        public int Days { get; set; } = 14;
    }

#nullable disable
}
