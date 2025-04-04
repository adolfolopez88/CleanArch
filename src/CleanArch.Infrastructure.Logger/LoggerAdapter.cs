using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace CleanArch.Infrastructure.Logger
{
    public class LoggerAdapter<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public LoggerAdapter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<T>();
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    public static class LoggerExtensions
    {
        public static IServiceCollection AddLoggerServices(this IServiceCollection services)
        {
            return services;
        }

        public static IHostBuilder UseSerilogLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());
        }
    }
}