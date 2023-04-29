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
    /// 前后台公用--发送兑换码
    /// </summary>
    public class SendDHCodeCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 兑换码(自动发送不需传入)
        /// </summary>
        public string DHCode { get; set; } = "";

        /// <summary>
        /// 物流公司名称(自动发送不需传入)
        /// </summary>
        public string CompanyName { get; set; } = "";

        /// <summary>
        /// 物流单号(自动发送不需传入)
        /// </summary>
        public string ExpressCode { get; set; } = "";

        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// 下单人Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 接收短信电话(上课电话|收货人电话)
        /// </summary>
        public string SendMsgMobile { get; set; }

        /// <summary>
        /// 课程Id
        /// </summary>
        public Guid CourseId { get; set; }
        
        /// <summary>
        /// 操作者
        /// </summary>
        public Guid? Creator { get; set; }
    }
}
