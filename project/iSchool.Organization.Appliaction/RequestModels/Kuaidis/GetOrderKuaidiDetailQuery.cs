using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 查询订单的快递详情
    /// </summary>
    public class GetOrderKuaidiDetailQuery : IRequest<OrderKuaidiDetailDto?>
    {        
        public Guid OrderId { get; set; }
    }

#nullable disable
}

