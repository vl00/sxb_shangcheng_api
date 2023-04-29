using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.OrgService_bg.ResponseModels
{
    public class BgMallFenleiSaveCmdResult
    {
        /// <summary>code</summary>
        public int Code { get; set; }
        /// <summary>排序</summary>
        public int Sort { get; set; }
        public int Depth { get; set; }
    }
}
