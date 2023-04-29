using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using MediatR;

namespace iSchool.Organization.Appliaction.OrgService_bg.Organization
{
    /// <summary>
    /// 根据机构Id，获取机构信息
    /// </summary>
    public class OrgInfoByIdQuery:IRequest<Domain.Organization>
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid Id { get; set; }
    }
}
