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
    /// 用户列表，【微信昵称】
    /// </summary>
    public class UserInfosByUserIdsOrMobileQuery : IRequest<List<UserInfoByUserIdsOrMobileResponse>>
    {   
        /// <summary>
        /// 下单人Id
        /// </summary>        
        public List<Guid>? UserIds { get; set; }

        /// <summary>
        /// 下单人手机号
        /// </summary>
        public string OrdMobile { get; set; }

    }

}
