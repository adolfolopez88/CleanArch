using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch.Infrastructure.WebServices
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add HTTP clients
            services.AddHttpClient<HttpClientBase>();
            
            // Configure base URL from configuration
            services.Configure<HttpClientSettings>(configuration.GetSection("HttpClientSettings"));
            
            // Register HTTP clients
            // Example: services.AddScoped<ISomeApiClient, SomeApiHttpClient>();
            
            return services;
        }
    }
    
    public class HttpClientSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
        public bool UseAuthentication { get; set; } = false;
        public string AuthScheme { get; set; } = "Bearer";
    }
}