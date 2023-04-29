using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 购买成功后,预给评测的获奖机会
    /// </summary>
    public class PresetEvltRewardChangesCmd : IRequest
    {
        /// <summary>订单id</summary>
        public Guid OrderId { get; set; }
        /// <summary>订单详情</summary>
        public OrderDetailQueryResult? OrderDetail { get; set; }

        /// <summary>上级(顾问)id</summary>
        public Guid? FxHeadUserId { get; set; }
        /// <summary>当前用户是否fx顾问</summary>
        public bool IsFxAdviser { get; set; } = false;

    }

    /// <summary>好物新人立返奖励</summary>
    public class NewUserRewardOfBuyGoodthingCmd : IRequest
    {
        /// <summary>预订单id</summary>
        public Guid AdvOrderId { get; set; }
        /// <summary>(全)订单详情</summary>
        public OrderDetailSimQryResult? OrdersEntity { get; set; }

        public IEnumerable<CourseDrpInfo>? CourseDrpInfos { get; set; }

    }

#nullable disable
}
