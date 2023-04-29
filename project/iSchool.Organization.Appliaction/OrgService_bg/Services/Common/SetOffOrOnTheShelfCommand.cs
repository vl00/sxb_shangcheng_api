using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service
{
    /// <summary>
    /// 后台通用设置status
    /// </summary>
    public class SetOffOrOnTheShelfCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 表唯一Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 需清除的相关缓存
        /// </summary>
        public List<string> BatchDelCache { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 操作者
        /// </summary>
        public Guid? UserId { get; set; }
    }
}
