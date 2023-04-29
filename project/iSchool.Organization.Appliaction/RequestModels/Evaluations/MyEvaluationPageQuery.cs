using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 分页查询我的评测列表
    /// </summary>
    public class MyEvaluationPageQuery : IRequest<LoadMoreResult<MyEvaluationItemDto>>
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; } = 1;
        /// <summary>
        /// 页大小，默认10
        /// </summary>
        public int PageSize { get; set; } = 10;
        /// <summary>
        /// 看别人的评测要传的用户ID
        /// </summary>

        public Guid? SeeUserId { get; set; }
    }
}
