using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels.Orders
{
    /// <summary>
    /// 批量一键发货
    /// </summary>
    public class BatchSendGoodsCommand:IRequest<ResponseResult>
    {
        public Guid UserId { get; set; }

        /// <summary>
        /// 订单Id集合
        /// </summary>
        public List<Guid> OrderIds { get; set; }
    }
}
