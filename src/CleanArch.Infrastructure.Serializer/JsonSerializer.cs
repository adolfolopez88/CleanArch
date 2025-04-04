using CleanArch.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CleanArch.Infrastructure.Serializer
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializer()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public string Serialize<T>(T obj)
        {
            return System.Text.Json.JsonSerializer.Serialize(obj, _options);
        }

        public T? Deserialize<T>(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, _options);
        }
    }

    public static class SerializerServiceExtensions
    {
        public static IServiceCollection AddSerializerServices(this IServiceCollection services)
        {
            services.AddSingleton<ISerializer, JsonSerializer>();
            return services;
        }
    }
}