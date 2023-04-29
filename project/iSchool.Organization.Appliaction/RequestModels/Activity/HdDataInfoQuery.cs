using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 新活动
    /// </summary>
    public class HdDataInfoQuery : IRequest<HdDataInfoDto>
    {
        /// <inheritdoc cref="HdDataInfoQuery"/>
        public HdDataInfoQuery() { }

        /// <summary>活动id</summary>
        public Guid Id { get; set; }
        /// <summary>活动码(有可能有推广码)</summary>
        public string? Code { get; set; }

        /// <summary>
        /// 0=默认优先查cache再查db<br/>
        /// -1=直接查db<br/>
        /// 1=查db再覆盖cache<br/>
        /// </summary>
        public int CacheMode { get; set; }
    }

#nullable disable
}
