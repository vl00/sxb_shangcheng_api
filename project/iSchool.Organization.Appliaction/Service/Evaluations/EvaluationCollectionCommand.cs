using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 收藏评测或取消
    /// </summary>
    public class EvaluationCollectionCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvaluationId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        ///// <summary>
        ///// 收藏/取消（true:收藏；false:取消）
        ///// </summary>
        //public bool AddOrCancel { get; set; }
    }

    /// <summary>
    /// 收藏评测或取消
    /// </summary>
    public class EvaluationCollectionRequest
    {
        /// <summary>
        /// 评测Id
        /// </summary>
        public Guid EvaluationId { get; set; }

    }
}
