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
    /// 删除专题并把专题下评测归为其他专题
    /// </summary>
    public class DelSpecialChangeEvltSpecCommand : IRequest<ResponseResult>
    {
        /// <summary>待删除专题Id/ </summary>
        public Guid DelId { get; set; }

        /// <summary>待删除专题的评测归为专题Id </summary>
        public Guid NewId { get; set; }

        /// <summary>专题类型（1：小专题；2：大专题；） </summary>
        public int SpecialType { get; set; }

        /// <summary>操作者 </summary>
        public Guid? UserId { get; set; }
    }
}
