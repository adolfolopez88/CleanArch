using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using CleanArch.Application.Behaviors;

namespace CleanArch.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            
            // Register validators
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Register behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            
            // Register application services
            // Example: services.AddScoped<ISomeAppService, SomeAppService>();
            
            return services;
        }
    }
}