using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 调用支付中心检查订单状态.<br/>
    /// 主要查询是否支付成功
    /// </summary>
    public class FinanceCheckOrderPayStatusQuery : IRequest<FinanceCheckOrderPayStatusQryResult>
    { 
        public Guid OrderId { get; set; }
        public int OrderType { get; set; } = 3;
    }
}

