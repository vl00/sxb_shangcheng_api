using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Organization.Api.Conventions
{
    public class ConvQueryStringValueProviderFactory : IValueProviderFactory
    {
        private HashSet<string> _keys;

        public ConvQueryStringValueProviderFactory() : this((IEnumerable<string>)null)
        {
        }

        public ConvQueryStringValueProviderFactory(IEnumerable<string> keys)
        {
            _keys = keys != null ? new HashSet<string>(keys) : null;
        }

        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            var query = context.ActionContext.HttpContext.Request.Query;
            var route = context.ActionContext.RouteData.Values;
            context.ValueProviders.Insert(0, new ConvQueryStringValueProvider(_keys, query, route));
            return Task.CompletedTask;
        }


        public void AddKey(string key)
        {
            if (_keys == null)
            {
                _keys = new HashSet<string>();
            }

            _keys.Add(key);
        }
    }
}
