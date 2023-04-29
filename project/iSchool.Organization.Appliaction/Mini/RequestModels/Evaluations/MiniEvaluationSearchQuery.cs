using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 根据id 查询种草集合
    /// </summary>
    public class MiniEvaluationSearchQuery : IRequest<List<MiniEvaluationItemDto>>
    {
        /// <summary>
        /// 种草id  list
        /// </summary>
        public List<Guid> Ids { get; set; }
    }
}
