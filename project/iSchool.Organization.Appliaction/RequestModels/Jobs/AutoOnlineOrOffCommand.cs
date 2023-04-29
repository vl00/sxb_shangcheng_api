using MediatR;
using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable
    /// <summary>
    /// 用于后台服务自动上线or下线
    /// </summary>
    public class AutoOnlineOrOffCommand : IRequest
    {
        public AutoOnlineOrOffContentType? ContentType { get; set; } = AutoOnlineOrOffContentType.Course;
    }
#nullable disable
}
