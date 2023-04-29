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
    /// [分销后台] 批量获取用户评测和购买课程info
    /// </summary>
    public class GetUserEvltAndBuyCourseInfoQuery : IRequest<IEnumerable<UserEvltAndBuyCourseInfoDto>>
    {
        public Guid[] UserIds { get; set; } = default!;
    }

#nullable disable
}
