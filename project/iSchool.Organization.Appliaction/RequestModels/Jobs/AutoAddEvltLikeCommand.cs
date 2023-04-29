using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable
    /// <summary>
    /// 用于后台服务自动添加评测点赞
    /// </summary>
    public class AutoLikeEvaluationCommand : IRequest
    {
        /// <summary>评测创建时间不低于此时间才能自动刷赞</summary>
        public DateTime? Nbf { get; set; } = new DateTime(2020, 11, 1, 0, 0, 0, DateTimeKind.Local);
        /// <summary>此时间段内只对精华评测刷赞</summary>
        public int[]? StickHours { get; set; } // [20,22]
    }
#nullable disable
}
