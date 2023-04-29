using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Organization.Activity.Api.Configurations
{
    public static class AutoMapperSetup
    {
        public static void AddAutoMapperSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddAutoMapper(new[] 
            {
                typeof(iSchool.Organization.Appliaction.AutoMapper.DomainToViewModelMappingProfile), 
                typeof(iSchool.Organization.Appliaction.AutoMapper.ViewModelToDomainMappingProfile),

                typeof(iSchool.Organization.Activity.Appliaction.AutoMapper.DomainToViewModelMappingProfile),
                typeof(iSchool.Organization.Activity.Appliaction.AutoMapper.ViewModelToDomainMappingProfile),
            });
        }
    }
}
