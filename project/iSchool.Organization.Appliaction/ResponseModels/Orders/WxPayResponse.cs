using iSchool.Infrastructure;
using iSchool.Organization.Appliaction.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class WxPayResponse
    {
        //public WxPayResult? AddPayOrderResponse { get; set; }
        public JToken? AddPayOrderResponse { get; set; }
    }

#nullable disable
}
