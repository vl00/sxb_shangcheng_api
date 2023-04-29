using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.Service.PointsMall
{
    public class PointsMallOptions
    {
        public static readonly string Config = "PointsMallOptions";

        public string BaseUrl { get; set; }

        public string InnerToken { get; set; }
    }
}
