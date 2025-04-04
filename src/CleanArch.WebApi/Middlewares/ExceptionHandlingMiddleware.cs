using CleanArch.Domain.Filters;
using System.Net;
using System.Text.Json;

namespace CleanArch.WebApi.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception has occurred");

            var statusCode = HttpStatusCode.InternalServerError;
            object response;

            if (exception is BusinessException businessException)
            {
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    Status = (int)statusCode,
                    Message = businessException.Message,
                    Detail = businessException.Detail ?? string.Empty
                };
            }
            else if (exception is HttpCustomException httpException)
            {
                statusCode = (HttpStatusCode)httpException.StatusCode;
                response = new
                {
                    Status = httpException.StatusCode,
                    Message = httpException.Message,
                    Detail = httpException.Detail ?? string.Empty
                };
            }
            else if (exception is InternalValidationException validationException)
            {
                statusCode = HttpStatusCode.UnprocessableEntity;
                response = new
                {
                    Status = (int)statusCode,
                    Message = "Validation failed",
                    Detail = validationException.Message,
                    Errors = validationException.Errors
                };
            }
            else
            {
                response = new
                {
                    Status = (int)statusCode,
                    Message = "Internal Server Error",
                    Detail = exception.Message
                };
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}