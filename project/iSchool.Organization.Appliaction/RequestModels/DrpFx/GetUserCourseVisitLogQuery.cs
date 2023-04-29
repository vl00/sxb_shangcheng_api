using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Appliaction.ResponseModels.DrpFx;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    

   
    public class GetUserCourseVisitLogQuery : IRequest<List<UserCourseVisitLog>>
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public List<Guid> UserIds { get; set; }
    }

#nullable disable
}
