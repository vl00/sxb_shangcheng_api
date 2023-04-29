using iSchool.Domain.Modles;
using iSchool.Organization.Appliaction.OrgService_bg.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.RequestModels
{
    public class BgMallFenleisDeleteCmd : IRequest<object>
    {
        public int Code { get; set; }

        public Guid UserId { get; set; }
    }
}
