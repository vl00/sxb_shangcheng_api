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
    /// 编辑评测通用方法
    /// </summary>
    public class EditEvalByIdWithFieldCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测Id
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

        /// <summary>
        /// 是否是评论表，是则tablename 不用传值
        /// </summary>
        public bool IsEval { get; set; } = true;

        /// <summary>
        /// 其他表名
        /// </summary>
        public string TableName { get; set; }
    }
}
