using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// mini 订单list
    /// </summary>
    public class MiniOrderPglistQryResult
    {
        public NameCodeDto<int>[] OrderStatusArr { get; set; } = default!;
        public PagedList<OrderItemDto> PageInfo { get; set; } = default!;        
    }

    /// <summary>
    /// mini 兑换记录订单list
    /// </summary>
    public class MiniPointsExchangeOrderPglistQryResult
    {
        public PagedList<OrderItemDto> PageInfo { get; set; } = default!;
    }
}
#nullable disable
