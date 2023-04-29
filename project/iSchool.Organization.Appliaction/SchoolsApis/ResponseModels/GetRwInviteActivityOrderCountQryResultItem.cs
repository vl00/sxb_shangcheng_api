using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class GetRwInviteActivityOrderCountQryResultItem
    {
        /// <summary>昵称(微信)</summary>
        public string Un_nickname { get; set; } = default!;
        public string UnionID { get; set; } = default!;
        /// <summary>昵称(上学帮账号)</summary>
        public string U_nickname { get; set; } = default!;
        public Guid Userid { get; set; }
        /// <summary>下单数</summary>
        public int Count { get; set; }
    }
}

#nullable disable
