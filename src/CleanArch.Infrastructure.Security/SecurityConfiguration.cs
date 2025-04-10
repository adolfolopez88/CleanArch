using CleanArch.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CleanArch.Infrastructure.Security
{
    public class SecurityConfiguration : ISecurityConfiguration
    {
        public string JwtSecret { get; set; } = string.Empty;
        public string JwtIssuer { get; set; } = string.Empty;
        public string JwtAudience { get; set; } = string.Empty;
        public int JwtExpirationMinutes { get; set; } = 60;
        public int RefreshTokenExpirationDays { get; set; } = 7;
        public string EncryptionKey { get; set; } = string.Empty;

        public static SecurityConfiguration FromConfiguration(IConfiguration configuration)
        {
            var jwtConfig = configuration.GetSection("JwtConfig");
            
            return new SecurityConfiguration
            {
                JwtSecret = jwtConfig["Secret"] ?? "YourVeryStrongSecretKeyWithAtLeast32Characters",
                JwtIssuer = jwtConfig["Issuer"] ?? "CleanArch",
                JwtAudience = jwtConfig["Audience"] ?? "CleanArchClient",
                JwtExpirationMinutes = int.TryParse(jwtConfig["ExpirationMinutes"], out int expirationMinutes) ? expirationMinutes : 60,
                RefreshTokenExpirationDays = int.TryParse(jwtConfig["RefreshTokenExpirationDays"], out int refreshTokenExpirationDays) ? refreshTokenExpirationDays : 7,
                EncryptionKey = jwtConfig["EncryptionKey"] ?? "YourEncryptionKey123456789012345678901234"
            };
        }
    }

    public static class SecurityServiceExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtConfig = configuration.GetSection("JwtConfig");
            var secret = jwtConfig["Secret"] ?? "YourVeryStrongSecretKeyWithAtLeast32Characters";
            var key = Encoding.ASCII.GetBytes(secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtConfig["Issuer"] ?? "CleanArch",
                    ValidAudience = jwtConfig["Audience"] ?? "CleanArchClient",
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }

        public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            var securityConfig = SecurityConfiguration.FromConfiguration(configuration);
            services.AddSingleton<ISecurityConfiguration>(securityConfig);
            services.AddSingleton(securityConfig);  // Register the concrete type as well
            services.AddScoped<ICryptoService, CryptoService>();
            services.AddScoped<IJwtService, JwtService>();

            return services;
        }
    }
}
