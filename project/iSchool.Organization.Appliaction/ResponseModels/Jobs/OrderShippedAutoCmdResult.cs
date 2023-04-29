using iSchool.Infrastructure;
using iSchool.Organization.Domain.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class OrderShippedAutoCmdResult
    {
        public Guid[]? NotOkOrderIds { get; set; }
        public string[]? Errs { get; set; }
    }

#nullable disable
}
