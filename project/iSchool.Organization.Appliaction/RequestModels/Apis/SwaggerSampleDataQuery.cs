using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
#nullable enable
    
    /// <summary>
    /// 
    /// </summary>
    public class SwaggerSampleDataQuery : IRequest<SwaggerSampleDataQueryResult>
    {
        public SwaggerSampleDataQuery() { }
        public SwaggerSampleDataQuery(string fname) => FileName = fname;

        public string FileName { get; set; } = default!;
    }    

#nullable disable
}
