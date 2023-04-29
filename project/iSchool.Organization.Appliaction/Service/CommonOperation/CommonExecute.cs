using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.CommonOperation
{
    /// <summary>
    /// 通用Execute
    /// </summary>
    public class CommonExecute : IRequest<ResponseResult>
    {
        /// <summary>
        /// 参数
        /// </summary>
        public DynamicParameters Parameters { get; set; }

        /// <summary>
        /// 执行sql
        /// </summary>
        public string Sql { get; set; }
    }
}
