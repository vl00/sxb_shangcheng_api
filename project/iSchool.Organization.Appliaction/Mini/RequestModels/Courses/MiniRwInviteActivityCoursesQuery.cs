using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class MiniRwInviteActivityCoursesQuery : IRequest<MiniRwInviteActivityCoursesQryResult>
    {
        /// <summary>
        /// 不传或null或0 都是查全部
        /// </summary>
        public int? City { get; set; }
    }
}
