// Composition/SwaggerSetup.cs
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace GpsApp.Composition
{
    public static class SwaggerSetup
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // Include XML comments (optional, for method descriptions)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                options.IncludeXmlComments(xmlPath);

                // JWT Bearer security definition
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Enter your JWT token. Do NOT include the 'Bearer' prefix.",

                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                };

                // Register the security scheme
                options.AddSecurityDefinition("Bearer", jwtSecurityScheme);

                // Require the security scheme globally (for [Authorize] endpoints)
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        jwtSecurityScheme,
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
    }
}

