using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Order
{
    /// <summary>
    /// (后台退款)订单退款
    /// </summary>
    public class RefundCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 课程Id
        /// </summary>
        [Obsolete]
        public Guid CourseId { get; set; }

        /// <summary>
        /// 预支付订单ID
        /// </summary>
        public Guid AdvanceOrderId { get; set; }

        /// <summary>
        /// 单号Id
        /// </summary>
        public Guid OrdId { get; set; }

        /// <summary>
        /// 退款金额
        /// </summary>
        [Obsolete]
        public double Price { get; set; }

        /// <summary>
        /// 退款api
        /// </summary>
        public string RefundApiUrl { get; set; }

        /// <summary>
        /// 下单人Id
        /// </summary>
        public Guid OrderUserId { get; set; }

        /// <summary>
        /// 操作者
        /// </summary>
        public Guid? UserId  { get; set; }


        public string PayKey { get; set; }

        public string System { get; set; }

        /// <summary>
        /// 退款数量
        /// </summary>
        public int RefundCount { get; set; }



    }
}
