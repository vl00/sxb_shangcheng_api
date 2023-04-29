using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.Apis;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 检查是否是顾问
    /// </summary>
    public class CheckIsFxHeadQuery : IRequest<CheckIsFxHeadQryResult>
    {
        public Guid UserId { get; set; }
    }

#nullable disable
}
