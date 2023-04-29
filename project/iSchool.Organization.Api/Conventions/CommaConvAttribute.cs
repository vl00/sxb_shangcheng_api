using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace iSchool.Organization.Api.Conventions
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class CommaConvAttribute : Attribute, IBindingSourceMetadata
    {
        public BindingSource BindingSource => BindingSource.Custom;
        public string Name { get; set; }
    }
}
