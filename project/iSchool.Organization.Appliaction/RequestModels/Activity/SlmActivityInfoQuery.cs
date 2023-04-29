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
    public class SlmActivityInfoQuery : IRequest<SlmActivityInfoDto>
    {
        /// <summary>活动码(有可能有推广码)</summary>
        public string Code { get; set; } = default!;
    }

#nullable disable
}
