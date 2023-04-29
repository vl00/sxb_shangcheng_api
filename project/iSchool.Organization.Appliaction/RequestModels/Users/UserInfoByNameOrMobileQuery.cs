using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class UserInfoByNameOrMobileQuery : IRequest<List<UserInfoByNameOrMobileResponse>>
    {
        public string Name { get; set; }

        public string Mobile { get; set; }
    }
}
