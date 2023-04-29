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
    public class ClaimOrgCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid OrgId { get; set; }

        /// <summary>
        /// 认证表Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 状态(1:待确定;2:已认领;3:拒绝;4:已取消;)
        /// </summary>
        public int Stats { get; set; }

    }
}
