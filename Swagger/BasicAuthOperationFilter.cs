using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace StudentPayments_API.Swagger
{
    public class BasicAuthOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Only apply to the OAuth token endpoint
            if (context.ApiDescription.RelativePath.Equals("api/oauth/token", StringComparison.OrdinalIgnoreCase))
            {
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Basic"
                                }
                            },
                            new string[] {}
                        }
                    }
                };
            }
        }
    }
}
