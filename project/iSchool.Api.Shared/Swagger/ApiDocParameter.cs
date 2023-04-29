using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Api.Swagger
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class ApiDocParameter : Attribute 
    {
        public ApiDocParameter() : this(null, null) { }

        public ApiDocParameter(Type type) : this(null, type) { }

        public ApiDocParameter(string name, Type type = null)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        public Type Type { get; set; }
        public ParameterLocation In { get; set; } = (ParameterLocation)(-1);
        public string Desc { get; set; }
        public bool Required { get; set; }
        public bool AllowEmptyValue { get; set; }
    }

    public class ApiDocParameterOperationFilter 
        : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            
        }
    }    
}
