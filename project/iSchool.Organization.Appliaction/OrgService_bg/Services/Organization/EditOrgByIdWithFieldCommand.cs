using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Organization
{
    /// <summary>
    /// 编辑机构通用方法
    /// </summary>
    public class UpdateSpecialStatusCommand:IRequest<ResponseResult>
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid OrgId { get; set; }
        
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
