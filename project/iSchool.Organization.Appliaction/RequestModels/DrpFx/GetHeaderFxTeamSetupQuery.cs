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
    /// 分销组建团队条件情况
    /// </summary>
    public class GetHeaderFxTeamSetupQuery : IRequest<HeaderFxTeamSetupInfoDto>
    { 
        /// <summary>(我)用户id</summary>
        public Guid UserId { get; set; }
    }

#nullable disable
}
