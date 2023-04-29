using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class CheckIsFxHeadQryResult
    {
        /// <summary>是否顾问</summary>
        public bool IsHead { get; set; }
        /// <summary>上级id</summary>
        public Guid HeadFxUserId { get; set; }
    }

#nullable disable
}
