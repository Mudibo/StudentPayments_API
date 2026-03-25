using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace StudentPayments_API.Swagger
{
    public class IdempotencyKeyHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new System.Collections.Generic.List<OpenApiParameter>();


            if (context.ApiDescription.HttpMethod == "POST" &&
                (context.ApiDescription.RelativePath.Contains("intent") ||
                 context.ApiDescription.RelativePath.Contains("notification")))
            {
                if (!operation.Parameters.Any(p => p.Name == "Idempotency-Key"))
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = "Idempotency-Key",
                        In = ParameterLocation.Header,
                        Required = true,
                        Schema = new OpenApiSchema { Type = "string" },
                        Description = "A unique key to ensure idempotency of the request"
                    });
                }
            }
        }
    }
}