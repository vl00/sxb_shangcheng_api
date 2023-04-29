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
    public sealed class BindFormFile : ModelBinderAttribute
    {
        public BindFormFile(int i) : this()
        {
            Idx = i;
        }

        public BindFormFile(string key) : this()
        {
            Key = key;
        }

        public BindFormFile() : base(typeof(FormFileModelBinder))
        {
            BindingSource = BindingSource.FormFile;
        }

        // public new Type BinderType => base.BinderType;
        // public new string Name => base.Name;

        public int? Idx { get; }
        public string Key { get; }

        class FormFileModelBinder : IModelBinder
        {            
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                var httpContext = bindingContext.HttpContext;
                var request = httpContext.Request;

                if (!request.HasFormContentType)
                {
                    throw new NotSupportedException("this is not a form request");
                }
                
                var metadata = bindingContext.ModelMetadata as DefaultModelMetadata;
                //var actionDescriptor = bindingContext.ActionContext.ActionDescriptor as ControllerActionDescriptor;
                //ModelAttributes.GetAttributesForParameter()

                if (bindingContext.ModelType == typeof(IFormFileCollection) || bindingContext.ModelType == typeof(IEnumerable<IFormFile>))
                {
                    bindingContext.Result = ModelBindingResult.Success(request.Form.Files);
                }
                else if (bindingContext.ModelType == typeof(IEnumerable<IFormFile>))
                {
                    bindingContext.Result = ModelBindingResult.Success(request.Form.Files);
                }
                else if (bindingContext.ModelType == typeof(IFormFile[]))
                {
                    bindingContext.Result = ModelBindingResult.Success(request.Form.Files.ToArray());
                }
                else if (typeof(IList<IFormFile>).IsAssignableFrom(bindingContext.ModelType))
                {
                    bindingContext.Result = ModelBindingResult.Success(request.Form.Files.ToList());
                }
                else
                {
                    var pattr = metadata.Attributes.ParameterAttributes.OfType<BindFormFile>().Single();
                    if (pattr.Idx != null) bindingContext.Result = ModelBindingResult.Success(request.Form.Files[pattr.Idx.Value]);
                    else bindingContext.Result = ModelBindingResult.Success(request.Form.Files[pattr.Key]);
                }
                return Task.CompletedTask;
            }
        }
    }
}
