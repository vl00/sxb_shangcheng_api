using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain.Modles;
using iSchool.Organization.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace iSchool.Organization.Appliaction.ResponseModels
{
    /// <summary>
    /// 
    /// </summary>
    public class RecvAddressPglistQueryResult
    {
        public PagedList<RecvAddressDto> PageInfo { get; set; } = default!;
    }
}
#nullable disable
