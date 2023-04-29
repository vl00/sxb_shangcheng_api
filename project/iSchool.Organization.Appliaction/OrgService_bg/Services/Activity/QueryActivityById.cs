using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg
{
    /// <summary>
    /// 待编辑活动信息
    /// </summary>
    public class QueryActivityById : IRequest<AddUpdateActivityShowDto>
    {
        /// <summary>
        /// 活动Id
        /// </summary>
        public Guid ActivityId { get; set; }
    }
}
