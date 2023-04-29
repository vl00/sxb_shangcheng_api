using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class GetRwInviteActivityOrderCountQuery : IRequest<GetRwInviteActivityOrderCountQryResultItem[]>
    {
        /// <summary>unionID</summary>
        public string[] UnionIDs { get; set; } = default!;

        /// <summary>
        /// 不传或null = 全部 <br/>
        /// 1 = 被发展人购买资格------>付费机会制 <br/>
        /// 2 = 发展人购买资格积分------>推广积分制
        /// </summary>
        public int? CourseExchangeType { get; set; } 
    }
}

#nullable disable
