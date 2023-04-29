using iSchool.Organization.Appliaction.RequestModels;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.Organization.Appliaction.Service.Order
{
    /// <summary>
    /// 用户订单查询请求实体Model
    /// </summary>
    public class OrdersByUserIdQuery:IRequest<List<OrdersByUserIdQueryResponse>>
    {
        ///// <summary>
        ///// 分页信息
        ///// </summary>
        //public PageInfo PageInfo { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid Userid { get; set; }
    }
}
