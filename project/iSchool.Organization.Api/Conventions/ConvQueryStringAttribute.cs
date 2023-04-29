using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Organization.Api.Conventions
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ConvQueryStringAttribute : Attribute, IResourceFilter
    {
        private readonly ConvQueryStringValueProviderFactory _factory;

        public ConvQueryStringAttribute()
        {
            _factory = new ConvQueryStringValueProviderFactory();
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {

        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            context.ValueProviderFactories.Insert(0, _factory);
        }

        public void AddKey(string parameterName)
        {
            _factory.AddKey(parameterName);
        }
    }
}
