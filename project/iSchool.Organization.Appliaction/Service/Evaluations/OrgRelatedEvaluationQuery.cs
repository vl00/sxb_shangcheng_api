using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Evaluations
{
    /// <summary>
    /// 机构相关评测
    /// </summary>
    public class OrgRelatedEvaluationQuery:IRequest<LoadMoreResult<EvaluationItemDto>>
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid OrgId { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小，默认10
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
