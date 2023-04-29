using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 调王宁API,获取用户列表
    /// </summary>
    public class UserInfosByAPICommand : IRequest<List<userinfo>>
    {   
        /// <summary>
        /// 用户Ids
        /// </summary>        
        public IEnumerable<Guid> UserIds { get; set; }
    }

}
