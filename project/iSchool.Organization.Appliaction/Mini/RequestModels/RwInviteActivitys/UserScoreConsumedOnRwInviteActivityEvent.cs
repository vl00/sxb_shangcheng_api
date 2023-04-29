using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using iSchool.Organization.Domain.Enum;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    public class UserScoreConsumedOnRwInviteActivityEvent : INotification
    {
        public UserSxbUnionIDDto? UnionID_dto = default!;
        public Guid UserId { get; set; }

        /// <summary>商品sku id</summary>
        public Guid GoodsId { get; set; }
        public CourseGoodsSimpleInfoDto? GoodsInfo { get; set; } = default!;

        public int? BuyCount { get; set; }
    }

#nullable disable
}
