using iSchool.Organization.Appliaction.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.ResponseModels
{
    public class BgHotMallFenleisLoadQueryResult
    {
        /// <summary>3个热门分类</summary>
        public IEnumerable<BgMallFenleisLoadQueryResult> Ls { get; set; }
    }
}
