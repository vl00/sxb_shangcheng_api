using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

namespace iSchool.Organization.Appliaction.RequestModels
{
    public class GetCourseInfosForSchoolsQuery : IRequest<GetCourseInfosForSchoolsQryResult>
    {
        /// <summary>course 长id</summary>
        public Guid[]? Ids { get; set; } = default!;


        /// <summary>
        /// course 短id
        /// </summary>
        public string[]? SIds { get; set; } = default!;

        /// <summary>
        /// 是否需要返回小程序二维码 <br/>
        /// 测试默认false, 正式默认true
        /// </summary>
#if DEBUG
        public bool Mp { get; set; } = false;
#else
        public bool Mp { get; set; } = true;
#endif
    }
}

#nullable disable
