using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CleanArch.Infrastructure.Mapper
{
    public static class MapperServiceExtensions
    {
        public static IServiceCollection AddMapperServices(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => 
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
                // Add any additional configuration here
            });
            
            return services;
        }
    }
}