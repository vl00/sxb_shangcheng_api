using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.EvaluationCrawler
{
    /// <summary>
    /// 编辑抓取评测通用方法
    /// </summary>
    public class EditEvaluationCrawlerByIdWithFieldCommand : IRequest<ResponseResult>
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
