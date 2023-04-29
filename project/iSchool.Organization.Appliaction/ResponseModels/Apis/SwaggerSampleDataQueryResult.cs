using iSchool.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable

    public class SwaggerSampleDataQueryResult
    {
        public string? JsonStr { get; set; }

        public T GetData<T>() => JsonStr.ToObject<T>();
    }

#nullable disable
}
