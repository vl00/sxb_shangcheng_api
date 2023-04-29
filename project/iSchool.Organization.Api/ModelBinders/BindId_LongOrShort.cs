using iSchool.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Api.ModelBinders
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class BindId_LongOrShort : ModelBinderAttribute
    {
        public BindId_LongOrShort(string path = "id") : this()
        {
            Path = path;
        }

        private BindId_LongOrShort() : base(typeof(ModelBinder))
        {
            BindingSource = BindingSource.Path;
        }

        public string Path { get; }

        class ModelBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                var httpContext = bindingContext.HttpContext;
                var request = httpContext.Request;
                var routeData = bindingContext.ActionContext.RouteData;
                var metadata = bindingContext.ModelMetadata as DefaultModelMetadata;

                var pattr = metadata.Attributes.ParameterAttributes.OfType<BindId_LongOrShort>().Single();
                var path = string.IsNullOrEmpty(pattr.Path) ? "id" : pattr.Path;

                var str_id = routeData.Values[path]?.ToString();
                if (string.IsNullOrEmpty(str_id))
                {
                    str_id = request.Query[path];
                }
                if (string.IsNullOrEmpty(str_id))
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                    return Task.CompletedTask;
                }

                object id = null;
                if (Guid.TryParse(str_id, out var _gid)) id = _gid;
                else if (UrlShortIdUtil.IsNumeric(str_id)) id = Convert.ToInt64(str_id);
                else
                {
                    var i = -1;
                    i = (i = str_id.IndexOf('1')) > -1 ? i :
                        (i = str_id.IndexOf('8')) > -1 ? i :
                        (i = str_id.IndexOf('9')) > -1 ? i :
                        (i = str_id.IndexOf('0')) > -1 ? i : -1;
                    if (i > -1) str_id = str_id[(i + 1)..];
                    id = UrlShortIdUtil.Base322Long(str_id);
                }

                bindingContext.Result = ModelBindingResult.Success(id);                
                return Task.CompletedTask;
            }
        }
    }
}
