using iSchool.Organization.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class GetWxPayFlowTypeQryResult
    { 
        public WxPayFlowTypeEnum Type { get; set; }

        public string AppId { get; set; } = default!;
    }

#nullable disable
}
