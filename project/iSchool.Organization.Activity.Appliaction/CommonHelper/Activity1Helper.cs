using iSchool.Organization.Appliaction.ViewModels;
using iSchool.Organization.Domain;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iSchool.Organization.Appliaction.CommonHelper
{
    public class Activity1Helper : IActivityHelper
    {
        public bool TryGetInfo(string code, out ActivityInfo activityInfo)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrWhiteSpace(code))
            {
                activityInfo = new ActivityInfo 
                { 
                    OriginCode = code, 
                    ActivityId = Guid.Parse(Consts.Activity1_Guid),
                    Acode = "h1",
                };
                return true;
            }

            activityInfo = null;
            var x = Regex.Match(code, @"^h(?<h>\d+)(t(?<t>\d+)){0,1}$", RegexOptions.IgnoreCase);
            var aid = x.Groups["h"].Value;
            var no = x.Groups["t"].Value;
            if (string.IsNullOrEmpty(aid)) return false;

            var i = Convert.ToInt32(aid);
            if (i != 1)
            {
                return false;
            }

            activityInfo = new ActivityInfo
            {
                OriginCode = code.ToLower(),
                ActivityId = Guid.Parse(Consts.Activity1_Guid),
                Acode = "h1",
                Promocode = !string.IsNullOrEmpty(no) ? $"h1t{no}".ToLower() : null,
                PromoNo = string.IsNullOrEmpty(no) ? null : no,
            };
            return true;
        }
    }
}
