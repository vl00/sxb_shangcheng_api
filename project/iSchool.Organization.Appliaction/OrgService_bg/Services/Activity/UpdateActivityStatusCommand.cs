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
    /// 活动上下架
    /// </summary>
    public class UpdateActivityStatusCommand : IRequest<ResponseResult>
    {
        /// <summary>
        /// 活动Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 活动状态（1:上架;2:下架;）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 操作者
        /// </summary>
        public Guid? UserId { get; set; }
    }
}
