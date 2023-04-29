using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 用户在评测里投票
    /// </summary>
    public class UserSelectEvltVoteCommand : IRequest<IEnumerable<UserSelectEvltVoteResult>>
    {
        /// <summary>
        /// 评测id
        /// </summary>
        public Guid EvltId { get; set; }
        /// <summary>
        /// 投票id
        /// </summary>
        public Guid VoteId { get; set; }
        /// <summary>
        /// 投票项id
        /// </summary>
        public Guid VoteItemId { get; set; }
        /// <summary>
        /// 包含评测+投票+投票项的关系,用于验证
        /// </summary>
        public string Token { get; set; }
    }
}
