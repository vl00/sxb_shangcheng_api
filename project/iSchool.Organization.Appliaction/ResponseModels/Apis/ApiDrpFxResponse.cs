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

    public class ApiDrpFxResponse
    {
        public object? Result { get; set; }

        public class GetConsultantRateQryResult
        {
            /// <summary>是否高级顾问</summary>
            public bool IsHighConsultant { get; set; }
            /// <summary>是否普通顾问</summary>
            public bool IsConsultant { get; set; }
            /// <summary>工资系数 小数</summary>
            public double Rate { get; set; }
        }

        public class AddFxOrderCmdResult
        {            
            /// <summary>是否顾问</summary>
            public bool IsConsulstant { get; set; }
            /// <summary>上级用户id</summary>
            public Guid ParentUserId { get; set; }
        }
    }

#nullable disable
}
