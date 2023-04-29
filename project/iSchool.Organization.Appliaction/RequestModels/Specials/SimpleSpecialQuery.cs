using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable
    /// <summary>
    /// 发评测后选择专题列表分页查询
    /// </summary>
    public class SimpleSpecialQuery : IRequest<List<SimpleSpecialDto>>
    {
        /// <summary>
        /// 活动码
        /// </summary>
        public string? Code { get; set; }
    }
#nullable disable
}
