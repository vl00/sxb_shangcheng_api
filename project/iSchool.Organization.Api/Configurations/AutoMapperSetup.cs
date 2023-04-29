using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace iSchool.Organization.Api.Configurations
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
            });
        }
    }
}
