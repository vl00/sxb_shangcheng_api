using Autofac;
using FluentValidation;
using iSchool.Organization.Appliaction.Service.Course;
using MediatR;
using MediatR.Pipeline;
using System.Reflection;

namespace iSchool.Organization.Api.Modules.AutofacModule
{
    /// <summary>
    /// Mediator 注入
    /// </summary>
    public class MediatorModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();

            var mediatrOpenTypes = new[]
            {
                typeof(IRequestHandler<,>),
                typeof(INotificationHandler<>),
                typeof(IValidator<>),
            };

            foreach (var mediatrOpenType in mediatrOpenTypes)
            {
               

                builder
                    .RegisterAssemblyTypes(typeof(CoursesByInfoQuery).GetTypeInfo().Assembly)
                    .AsClosedTypesOf(mediatrOpenType)
                    .AsImplementedInterfaces();

                //builder
                //    .RegisterAssemblyTypes(typeof(CourseQuery).GetTypeInfo().Assembly)
                //    .AsClosedTypesOf(mediatrOpenType)
                //    .AsImplementedInterfaces();

                //builder
                //     .RegisterAssemblyTypes(typeof(DomainNotificationHandler).GetTypeInfo().Assembly)
                //     .AsClosedTypesOf(mediatrOpenType)
                //     .AsImplementedInterfaces();
            }

            builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));

            builder.Register<ServiceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
        }
    }
}
