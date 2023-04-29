using iSchool.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class WalletInsideUnFreezeAmountApiResult
    {
        public string? ErrorDesc { get; set; }

        public bool Success { get; set; }
    }

#nullable disable
}
