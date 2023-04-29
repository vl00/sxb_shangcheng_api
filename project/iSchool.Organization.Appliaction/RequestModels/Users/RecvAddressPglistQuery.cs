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
    /// 我的收货地址s列表
    /// </summary>
    public class RecvAddressPglistQuery : IRequest<RecvAddressPglistQueryResult>
    {   
        /// <summary>
        /// 账号id.前端可以不存.
        /// </summary>        
        public Guid UserId { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

#nullable disable
}
