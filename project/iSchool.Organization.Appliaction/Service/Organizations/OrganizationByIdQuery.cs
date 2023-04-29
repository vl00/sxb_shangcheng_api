using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.Organization
{

    public class OrganizationByIdQuery:IRequest<ResponseResult>
    {
        /// <summary>
        /// 机构Id
        /// </summary>
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// 机构短Id
        /// </summary>
        public long No { get; set; }
    }

   

}
