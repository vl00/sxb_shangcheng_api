using iSchool.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    /// <summary>
    /// 
    /// </summary>
    public class UserScoreOnRwInviteActivityResult
    {
        public object Result { get; set; } = default!;

        public T GetResult<T>() => (T)Result;
    }

#nullable disable
}
