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

    /// <summary>添加地址</summary>
    public class AddRecvAddressCommand : IRequest<RecvAddressDto?>
    {
        /// <summary>
        /// 账号id.前端可以不存.
        /// </summary>        
        public Guid UserId { get; set; }

        public RecvAddressDto AddressDto { get; set; } = default!;
    }


#nullable disable
}
