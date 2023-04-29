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
    /// 批量查询用户s基本信息
    /// </summary>
    public class UserSimpleInfoQuery : IRequest<IEnumerable<UserSimpleInfoQueryResult>>
    {
        public IEnumerable<Guid> UserIds { get; set; }
    }
}
