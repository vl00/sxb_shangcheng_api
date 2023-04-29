using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    [Obsolete("旧活动")]
    public class ActivitySimpleInfoQuery : IRequest<ActivitySimpleInfoDto?>
    {
        /// <summary>活动id</summary>
        public Guid Id { get; set; }
    }

#nullable disable
}
