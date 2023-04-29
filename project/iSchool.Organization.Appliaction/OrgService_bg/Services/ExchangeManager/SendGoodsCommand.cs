using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels.Courses;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.ExchangeManager
{

    /// <summary>
    /// 后台--发货
    /// </summary>
    public class SendGoodsCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 是否发送兑换码
        /// </summary>
        public bool IsSendDHCode { get; set; }

        /// <summary>
        /// 是否发物流
        /// </summary>
        public bool IsSendExpress { get; set; } = true;

        /// <summary>
        /// 兑换码
        /// </summary>
        public string DHCode { get; set; }

        /// <summary>
        /// 物流公司编码
        /// </summary>
        public string ExpressType { get; set; }

        /// <summary>
        /// 物流单号
        /// </summary>
        public string ExpressCode { get; set; }

        /// <summary>
        /// 快递公司名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid OrderId { get; set; }
        /// <summary>
        /// 订单详情id
        /// </summary>
        public Guid OrderDetailId { get; set; }

        /// <summary>
        /// 下单人Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 上课电话(用于发短信)
        /// </summary>
        public string BeginClassMobile { get; set; }

        /// <summary>
        /// 收货人手机号
        /// </summary>
        public string RecvMobile { get; set; }

        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// 课程名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 操作者
        /// </summary>
        public Guid? Creator { get; set; }

    }
}
