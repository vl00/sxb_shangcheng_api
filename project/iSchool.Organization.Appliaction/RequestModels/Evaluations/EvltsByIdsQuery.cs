using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 学校--评测列表
    /// </summary>
    public class EvltsByIdsQuery : IRequest<ResponseResult>
    {
        /// <summary>
        /// 评测Id集
        /// </summary>
        public List<Guid> EvltIds { get; set; }
    }
}
