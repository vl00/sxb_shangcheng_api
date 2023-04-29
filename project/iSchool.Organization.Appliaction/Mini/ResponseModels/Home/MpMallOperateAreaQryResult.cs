using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class MpMallOperateAreaQryResult
    {
        /// <summary>限时闪购s</summary>
        public IEnumerable<MpCourseDataDto>? LimitedTimeOffers { get; set; }
        /// <summary>新人专享s</summary>
        public IEnumerable<MpCourseDataDto>? NewUserExclusives { get; set; }

        /// <summary>热销榜单s</summary>
        public IEnumerable<MpCourseDataDto>? HotSells { get; set; }
        /// <summary>本周上新s</summary>
        public IEnumerable<MpCourseDataDto>? NewOnWeeks { get; set; }

    }

#nullable disable
}
