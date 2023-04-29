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
    /// 专题上下架
    /// </summary>
    public class UpdateSpecialStatusCommand:IRequest<ResponseResult>
    {
        /// <summary>
        /// 专题Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 专题状态（1:上架;2:下架;）
        /// </summary>
        public int Status { get; set; }


        /// <summary>
        /// 操作者
        /// </summary>
        public Guid? UserId { get; set; }
    }
}
