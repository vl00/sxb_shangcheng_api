using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 用户在评测里投票结果
    /// </summary>
    public class UserSelectEvltVoteResult
    {
        /// <summary>
        /// 投票选项id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 票数
        /// </summary>
        public int Count { get; set; }
    }
}
