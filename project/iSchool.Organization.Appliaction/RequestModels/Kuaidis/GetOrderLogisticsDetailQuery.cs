using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{

    /// <summary>
    /// 查询多包物流的快递详情
    /// </summary>
    public class GetOrderLogisticsDetailQuery : IRequest<KuaidiDetailDto?>
    {
        public Guid LogisticId { get; set; }
    }
}
