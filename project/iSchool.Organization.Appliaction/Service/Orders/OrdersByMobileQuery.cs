using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Order
{
    /// <summary>
    /// 根据收货人手机号、下单人手机号，查询(收货用户信息及其小课订单信息)请求实体Model
    /// </summary>
    public class OrdersByMobileQuery: IRequest<List<OrdersByMobileQueryResponse>>
    {
        /// <summary>
        /// 收货人手机号
        /// </summary>
        public string RecvMobile { get; set; }

        /// <summary>
        /// 下单人手机号
        /// </summary>
        public string OrderMobile { get; set; }

    }
}
