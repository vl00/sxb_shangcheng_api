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
    /// 更新单字段后台通用
    /// </summary>
    public class ChangeSingleFieldCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 表唯一Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 字段名
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 字段值
        /// </summary>
        public object FieldValue { get; set; }

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
