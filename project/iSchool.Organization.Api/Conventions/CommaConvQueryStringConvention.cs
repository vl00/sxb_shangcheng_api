using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Organization.Api.Conventions
{
    public class CommaConvQueryStringConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            ConvQueryStringAttribute attribute = null;
            foreach (var parameter in action.Parameters)
            {
                if (parameter.Attributes.OfType<CommaConvAttribute>().Any())
                {
                    if (attribute == null)
                    {
                        attribute = new ConvQueryStringAttribute();
                        parameter.Action.Filters.Add(attribute);
                    }
                    attribute.AddKey(parameter.ParameterName);
                }
            }
        }
    }
}

