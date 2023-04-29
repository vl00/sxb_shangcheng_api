using iSchool.Organization.Appliaction.ResponseModels;
using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable

    /// <summary>
    /// 商城首页新的商品列表(含网课和好物)
    /// </summary>
    public class MpMallHomeCoursePageLsQuery : PageInfo, IRequest<CoursesByOrgIdQueryResponse>
    {
        /// <summary>
        /// 是否排查网课
        /// </summary>
        public bool ExcludeCourseType1 { get; set; }
    }

#nullable disable
}
