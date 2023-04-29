using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 用户s手机信息
    /// </summary>
    public class UserMobileInfoQuery : IRequest<(userinfo UserInfo, userinfo[] OtherUserInfo)[]>
    {
        public Guid[] UserIds { get; set; } = default!;
    }

#nullable disable
}
