using Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.Evaluations
{
    /// <summary>
    /// 评测上下架
    /// </summary>
    public class EvltSetOffOrOnTheShelfCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测一Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
               
        /// <summary>
        /// 操作者
        /// </summary>
        public Guid? UserId { get; set; }
    }
}
