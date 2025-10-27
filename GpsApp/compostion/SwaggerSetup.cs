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

                // API Information
                options.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = "GPS App V2 API",
                    Version = "v2",
                    Description = "Complete shipment workflow management system with domain-driven design. Supports Swedish address format (e.g., Tegelgatan 12, Stockholm, 113 58, Sverige).",
                    Contact = new OpenApiContact
                    {
                        Name = "GPS App Team",
                        Email = "jennifer.got@chasacademy.se"
                    }
                });

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

                // Basic Authentication security definition
                var basicSecurityScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    Description = "Basic Authentication for gateway devices. Format: gateway-id:password",

                    Reference = new OpenApiReference
                    {
                        Id = "Basic",
                        Type = ReferenceType.SecurityScheme
                    }
                };

                // Register the security schemes
                options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
                options.AddSecurityDefinition("Basic", basicSecurityScheme);

                // Require the security schemes globally
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        jwtSecurityScheme,
                        Array.Empty<string>()
                    },
                    {
                        basicSecurityScheme,
                        Array.Empty<string>()
                    }
                });

                // Enable annotations for better API documentation
                options.EnableAnnotations();
            });

            return services;
        }
    }
}
