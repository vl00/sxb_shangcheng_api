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

    /// <summary>删除地址</summary>
    public class DeleteRecvAddressCommand : IRequest<bool>
    {   
        /// <summary>
        /// 账号id.前端可以不存.
        /// </summary>        
        public Guid UserId { get; set; }
        public Guid Addressid { get; set; }
    }

#nullable disable
}
