// Composition/SwaggerSetup.cs
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;


namespace GpsApp.Composition
{
    public static class SwaggerSetup
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                options.IncludeXmlComments(xmlPath);
            });

            return services;
        }
    }
}
