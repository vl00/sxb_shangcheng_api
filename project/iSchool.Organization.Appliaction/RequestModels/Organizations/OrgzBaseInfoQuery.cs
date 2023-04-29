using iSchool.Organization.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 机构详情 -- base info
    /// </summary>
    public class OrgzBaseInfoQuery : IRequest<iSchool.Organization.Domain.Organization>
    {
        public long No { get; set; }
        public Guid OrgId { get; set; }


        public bool AllowNotValid { get; set; } = false;
    }
}
#nullable disable