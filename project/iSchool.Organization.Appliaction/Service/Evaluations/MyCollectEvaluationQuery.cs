using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 我收藏的评测
    /// </summary>
    public  class MyCollectEvaluationQuery : IRequest<EvaluationLoadMoreQueryResult>
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }
    }
}
