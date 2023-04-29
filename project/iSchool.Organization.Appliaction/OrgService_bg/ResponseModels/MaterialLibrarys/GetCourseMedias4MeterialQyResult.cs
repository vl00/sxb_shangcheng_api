using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.ResponseModels
{
    public class GetCourseMedias4MeterialQyResult
    {
        public string VideoUrl { get; set; }
        public string VideoCoverUrl { get; set; }

        public string[] Banners { get; set; }
    }
}
