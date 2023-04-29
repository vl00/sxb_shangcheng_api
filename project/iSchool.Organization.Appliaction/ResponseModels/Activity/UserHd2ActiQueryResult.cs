using iSchool.Organization.Appliaction.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <inheritdoc cref="UserHd2ActiQuery"/>
    public class UserHd2ActiQueryResult
    {
        /// <summary>本账号id</summary>
        public Guid UserId { get; set; }
        /// <summary>其他同手机账号id</summary>
        public Guid[] OtherUserIds { get; set; } = default!;
        /// <summary>手机号</summary>
        public string Mobile { get; set; } = default!;

        /// <summary>本次活动本手机号产生的评测数</summary>
        public int Allcount { get; set; }
        /// <summary>本次活动本账号产生的评测数</summary>
        public int Ucount { get; set; }
        /// <summary>本次活动其他同手机账号产生的评测数</summary>
        public int Ocount { get; set; }

        /// <summary>本次今日本手机号产生的评测数</summary>
        public int Allcount_now { get; set; }
        /// <summary>本次今日本账号产生的评测数</summary>
        public int Ucount_now { get; set; }
        /// <summary>本次今日其他同手机账号产生的评测数</summary>
        public int Ocount_now { get; set; }

        public Guid ActivityId { get; set; }
    }

#nullable disable
}
