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
    /// 买课后判断是否满49元,如是,通知升级为顾问
    /// </summary>
    public class CheckAndNotifyUserToDoFxlvupCmd : IRequest
    {
        public Guid UserId { get; set; }
    }

#nullable disable
}
