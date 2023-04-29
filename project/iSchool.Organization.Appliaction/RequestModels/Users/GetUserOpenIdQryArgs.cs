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

    /// <summary>获取用户openid</summary>
    public class GetUserOpenIdQryArgs : IRequest<(string, string)>
    {
        /// <summary>
        /// 用户id.
        /// </summary>        
        public Guid UserId { get; set; }
    }


#nullable disable
}
