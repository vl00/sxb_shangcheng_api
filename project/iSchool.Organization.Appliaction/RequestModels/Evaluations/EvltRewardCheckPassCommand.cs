using System;
using System.Collections.Generic;
using System.Text;
using iSchool.Organization.Domain;
using MediatR;

namespace iSchool.Organization.Appliaction.RequestModels.Evaluations
{
    /// <summary>
    /// 检验用户种草发放奖励条件是否符合
    /// </summary>
    public  class EvltRewardCheckPassCommand: IRequest<EvaluationReward>
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvltId { get; set; }

        /// <summary>
        /// (种草)用户Id
        /// </summary>
        public Guid UserId { get; set; }



    }
}
