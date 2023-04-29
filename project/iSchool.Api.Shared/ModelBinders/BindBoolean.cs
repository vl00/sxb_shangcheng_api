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
    /// <summary>
    /// 可以把url上面的1|0|true|false转为后端真正的bool类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class BindBoolean : ModelBinderAttribute
    {
        public BindBoolean() : base(typeof(ModelBinder))
        {
            BindingSource = BindingSource.Query;
        }        

        class ModelBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                var httpContext = bindingContext.HttpContext;
                var request = httpContext.Request;
                var routeData = bindingContext.ActionContext.RouteData;
                var metadata = bindingContext.ModelMetadata as DefaultModelMetadata;
                var actionDescriptor = bindingContext.ActionContext.ActionDescriptor as ControllerActionDescriptor;

                //var pattr = metadata.Attributes.ParameterAttributes.OfType<BindBoolean>().Single();
                var fdName = metadata.ParameterName;
                var pi = actionDescriptor.MethodInfo?.GetParameters()?.FirstOrDefault(_ => string.Equals(_.Name, fdName, StringComparison.OrdinalIgnoreCase));

                var str_v = routeData.Values[fdName]?.ToString();
                if (string.IsNullOrEmpty(str_v))
                {
                    str_v = request.Query[fdName];
                }

                var v = str_v == "1" ? true :
                    str_v == "0" ? false :
                    !string.IsNullOrEmpty(str_v) ? Convert.ToBoolean(str_v) :
                    pi == null || (Nullable.GetUnderlyingType(pi.ParameterType) != null) ? (object)null : // str_v is null or ''
                    pi.HasDefaultValue ? pi.DefaultValue : false;

                bindingContext.Result = ModelBindingResult.Success(v);                
                return Task.CompletedTask;
            }
        }


    }
}
