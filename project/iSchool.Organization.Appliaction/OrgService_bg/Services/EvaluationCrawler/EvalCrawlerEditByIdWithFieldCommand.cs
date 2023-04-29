using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.EvaluationCrawler
{   
    /// <summary>
    /// 抓取评测通过编辑方法
    /// </summary>
    public class EvalCrawlerEditByIdWithFieldCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public DynamicParameters Parameters { get; set; }

        /// <summary>
        /// Set 示例：logo=@logo
        /// </summary>
        public string UpdateSql { get; set; }
    }

}
