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
    /// 分页查询评测主页下的评测列表
    /// </summary>
    public class EvaluationLoadMoreQuery : IRequest<EvaluationLoadMoreQueryResult>
    {
        /// <summary>
        /// 科目 首页的其他传值-1
        /// </summary>
        public string Subj { get; set; }
        /// <summary>
        /// 年龄范围
        /// </summary>
        public string Age { get; set; }
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 页大小，默认10
        /// </summary>
        //public int PageSize { get; set; } = 10;

        /// <summary>
        /// 是否推荐
        /// </summary>
        public int Stick { get; set; }
    }
}
