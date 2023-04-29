using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    
    /// <summary>
    /// 种草奖励审核通过
    /// </summary>
    public class EvltRewardPassAuditCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 后台操作人
        /// </summary>
        public Guid Operator { get; set; }

        /// <summary>
        /// (种草)用户Id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// 种草奖励
        /// </summary>
        public decimal Reward { get; set; }
    }
}
