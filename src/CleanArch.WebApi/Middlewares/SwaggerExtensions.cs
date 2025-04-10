using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace CleanArch.WebApi.Middlewares
{
    public class SwaggerDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Add any document-level customizations here
        }
    }

    public class SwaggerOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var descriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor == null) return;

            // Get operation details
            var httpMethod = context.ApiDescription.HttpMethod ?? "GET";
            var path = operation.Tags.Count > 0 ? operation.Tags[0].Name : string.Empty;
            var endpoint = context.ApiDescription.RelativePath ?? string.Empty;
            var parameters = operation.Parameters;
            var requestBody = operation.RequestBody;

            // Build cURL command
            var curlBuilder = new StringBuilder($"curl -X {httpMethod} \\");
            curlBuilder.AppendLine();
            curlBuilder.Append($"  '{GetBaseUrl()}/{endpoint}'");

            // Add headers
            curlBuilder.AppendLine(" \\");
            curlBuilder.AppendLine("  -H 'Accept: application/json' \\");
            curlBuilder.AppendLine("  -H 'Content-Type: application/json'");

            // Add Authorization if required
            if (operation.Security != null && operation.Security.Count > 0)
            {
                curlBuilder.AppendLine(" \\");
                curlBuilder.AppendLine("  -H 'Authorization: Bearer YOUR_TOKEN'");
            }

            // Add query parameters
            if (parameters != null && parameters.Count > 0 && httpMethod == "GET")
            {
                var queryParams = parameters.Where(p => p.In == ParameterLocation.Query);
                if (queryParams.Any())
                {
                    curlBuilder.AppendLine(" \\");
                    curlBuilder.Append($"  -G");
                    foreach (var param in queryParams)
                    {
                        curlBuilder.AppendLine(" \\");
                        curlBuilder.Append($"  --data-urlencode '{param.Name}=value'");
                    }
                }
            }

            // Add path parameters (replace in URL)
            if (parameters != null)
            {
                foreach (var param in parameters.Where(p => p.In == ParameterLocation.Path))
                {
                    curlBuilder.Replace($"{{{param.Name}}}", $"value");
                }
            }

            // Add request body for POST/PUT
            if (requestBody != null && (httpMethod == "POST" || httpMethod == "PUT" || httpMethod == "PATCH"))
            {
                curlBuilder.AppendLine(" \\");
                curlBuilder.Append("  -d '{");
                if (requestBody.Content != null && 
                    requestBody.Content.TryGetValue("application/json", out var mediaType) && 
                    mediaType.Schema != null &&
                    mediaType.Schema.Properties != null &&
                    mediaType.Schema.Properties.Count > 0)
                {
                    var props = mediaType.Schema.Properties;
                    int count = 0;
                    foreach (var prop in props)
                    {
                        count++;
                        var valueExample = GetExampleValue(prop.Value);
                        curlBuilder.Append($"\"{prop.Key}\": {valueExample}");
                        if (count < props.Count)
                            curlBuilder.Append(", ");
                    }
                }
                curlBuilder.Append("}'");
            }

            // Add cURL example to operation extensions
            if (!operation.Extensions.ContainsKey("x-curl-example"))
            {
                var curlExample = new OpenApiString(curlBuilder.ToString());
                operation.Extensions.Add("x-curl-example", curlExample);
            }
        }

        private string GetBaseUrl()
        {
            return "http://localhost:8080";  // Adjust as needed
        }

        private string GetExampleValue(OpenApiSchema schema)
        {
            if (schema.Example != null)
            {
                return schema.Example is OpenApiString ? $"\"{schema.Example}\"" : schema.Example.ToString();
            }

            if (schema.Default != null)
            {
                return schema.Default is OpenApiString ? $"\"{schema.Default}\"" : schema.Default.ToString();
            }

            if (schema.Type == "string")
            {
                if (schema.Format == "date-time")
                    return "\"2023-01-01T00:00:00Z\"";
                return "\"example\"";
            }
            else if (schema.Type == "integer" || schema.Type == "number")
            {
                return "0";
            }
            else if (schema.Type == "boolean")
            {
                return "false";
            }
            else if (schema.Type == "array")
            {
                return "[]";
            }
            else if (schema.Type == "object" || schema.Reference != null)
            {
                return "{}";
            }

            return "null";
        }
    }
}